using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ProcSim.Core.Enums;
using ProcSim.Core.Factories;
using ProcSim.Core.Scheduling.Algorithms;
using ProcSim.Core.Simulation;
using System.Collections.ObjectModel;

namespace ProcSim.ViewModels;

public partial class VmSettingsViewModel : ObservableObject
{
    private readonly IRepositoryBase<VmConfig> _configRepo;

    public VmSettingsViewModel(IRepositoryBase<VmConfig> configRepo)
    {
        _configRepo = configRepo;

        // Inicializa a configuração da VM e os processos
        VmConfig = new VmConfig();

        // Inicializa os dispositivos disponíveis (considerando, por exemplo, Disk e Memory)
        AvailableDevices = new ObservableCollection<DeviceViewModel>
        {
            new DeviceViewModel { Name = "Disco", DeviceType = IoDeviceType.Disk, IsSelected = false, Channels = 1 },
            new DeviceViewModel { Name = "Memória", DeviceType = IoDeviceType.Memory, IsSelected = false, Channels = 1 }
            // Poderiam ser adicionados outros dispositivos, se necessário.
        };

        AvailableIoDevices = [.. Enum.GetValues<IoDeviceType>()];
        SchedulingAlgorithms = [.. Enum.GetValues<SchedulingAlgorithmType>()];

        SaveConfigCommand = new AsyncRelayCommand(async () => await SaveConfigAsync());
        LoadConfigCommand = new AsyncRelayCommand(async () => await LoadConfigAsync());

        SelectedAlgorithmInstance = SchedulingAlgorithmFactory.Create(SelectedAlgorithm);
        Quantum = 1;
        CanChangeAlgorithm = true;
    }

    public List<SchedulingAlgorithmType> Algorithms { get; } = [.. Enum.GetValues<SchedulingAlgorithmType>()];

    public VmConfig VmConfig { get; set; }

    public ObservableCollection<DeviceViewModel> AvailableDevices { get; set; }
    public ObservableCollection<IoDeviceType> AvailableIoDevices { get; set; }
    public ObservableCollection<SchedulingAlgorithmType> SchedulingAlgorithms { get; set; }

    public IAsyncRelayCommand SaveConfigCommand { get; }
    public IAsyncRelayCommand LoadConfigCommand { get; }

    public SchedulingAlgorithmType SelectedAlgorithm
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                SelectedAlgorithmInstance = SchedulingAlgorithmFactory.Create(value);

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPreemptive));
                OnPropertyChanged(nameof(SelectedAlgorithmInstance));
            }
        }
    } = SchedulingAlgorithmType.Fcfs;

    public ISchedulingAlgorithm SelectedAlgorithmInstance { get; private set; }

    public bool IsPreemptive => SelectedAlgorithmInstance is IPreemptiveAlgorithm;

    public int Quantum
    {
        get;
        set
        {
            if (field != value && value > 0)
            {
                field = value;
                OnPropertyChanged();
                UpdateSchedulerQuantum();
            }
        }
    }

    [ObservableProperty]
    public partial bool CanChangeAlgorithm { get; set; }

    private void UpdateSchedulerQuantum()
    {
        if (SelectedAlgorithmInstance is IPreemptiveAlgorithm preemptiveAlgorithm)
        {
            preemptiveAlgorithm.Quantum = Quantum;
        }
    }

    private async Task SaveConfigAsync()
    {
        VmConfig.Devices = [.. AvailableDevices.Where(d => d.IsSelected).Select(d => d.MapToDeviceConfig())];

        var dialog = new SaveFileDialog { Filter = _configRepo.FileFilter };
        dialog.ShowDialog();
        string filePath = dialog.FileName;

        if (!string.IsNullOrEmpty(filePath))
            await _configRepo.SaveAsync(VmConfig, filePath);
    }

    private async Task LoadConfigAsync()
    {
        var dialog = new OpenFileDialog { Filter = _configRepo.FileFilter };
        bool? result = dialog.ShowDialog();
        string filePath = dialog.FileName;

        if (string.IsNullOrEmpty(filePath))
            return;

        var config = await _configRepo.LoadAsync(filePath);
        if (config is null)
            return;

        VmConfig = config;
        // Atualiza os dispositivos disponíveis com base na configuração carregada.
        foreach (var deviceVM in AvailableDevices)
        {
            // Verifica se existe um dispositivo do mesmo tipo na configuração.
            var loadedDevice = VmConfig.Devices?.FirstOrDefault(d => d.DeviceType == deviceVM.DeviceType);
            if (loadedDevice is null)
            {
                deviceVM.IsSelected = false;
                continue;
            }

            deviceVM.IsSelected = true;
            // Supondo que os dispositivos para disco e memória possuam a propriedade Channels.
            // Se necessário, faça um cast para o tipo específico (ex.: DiskDevice ou MemoryDevice).
            deviceVM.Channels = loadedDevice.Channels;
        }
    }
}
