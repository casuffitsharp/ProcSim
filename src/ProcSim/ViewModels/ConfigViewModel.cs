using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ProcSim.Core.Enums;
using ProcSim.Core.Models;
using ProcSim.Core.Models.Operations;
using ProcSim.Core.Simulation;

namespace ProcSim.ViewModels;

public partial class ConfigViewModel : ObservableObject
{
    public VmConfig VmConfig { get; set; }

    public ObservableCollection<Process> Processes { get; set; }

    [ObservableProperty]
    public partial Process SelectedProcess { get; set; }

    public ObservableCollection<DeviceViewModel> AvailableDevices { get; set; }
    public ObservableCollection<IoDeviceType> AvailableIoDevices { get; set; }
    public ObservableCollection<SchedulingAlgorithmType> SchedulingAlgorithms { get; set; }

    public ICommand SaveVmConfigCommand { get; }
    public ICommand LoadVmConfigCommand { get; }

    public ICommand SaveProcessesConfigCommand { get; }
    public ICommand LoadProcessesConfigCommand { get; }
    public ICommand AddProcessCommand { get; }
    public ICommand RemoveProcessCommand { get; }
    public ICommand AddOperationCommand { get; }
    public ICommand RemoveOperationCommand { get; }

    private readonly IRepositoryBase<VmConfig> _vmConfigRepo;
    private readonly IRepositoryBase<List<Process>> _processConfigRepo;

    public ConfigViewModel(IRepositoryBase<VmConfig> vmConfigRepo, IRepositoryBase<List<Process>> processConfigRepo)
    {
        _vmConfigRepo = vmConfigRepo;
        _processConfigRepo = processConfigRepo;

        // Inicializa a configuração da VM e os processos
        VmConfig = new VmConfig();
        Processes = new ObservableCollection<Process>();

        // Inicializa os dispositivos disponíveis (considerando, por exemplo, Disk e Memory)
        AvailableDevices = new ObservableCollection<DeviceViewModel>
        {
            new DeviceViewModel { Name = "Disco", DeviceType = IoDeviceType.Disk, IsSelected = false, Channels = 1 },
            new DeviceViewModel { Name = "Memória", DeviceType = IoDeviceType.Memory, IsSelected = false, Channels = 1 }
            // Poderiam ser adicionados outros dispositivos, se necessário.
        };

        AvailableIoDevices = [.. Enum.GetValues<IoDeviceType>()];
        SchedulingAlgorithms = [.. Enum.GetValues<SchedulingAlgorithmType>()];

        // Inicializa os comandos
        SaveVmConfigCommand = new RelayCommand(async () => await SaveVmConfigAsync());
        LoadVmConfigCommand = new RelayCommand(async () => await LoadVmConfigAsync());
        SaveProcessesConfigCommand = new RelayCommand(async () => await SaveProcessesConfigAsync());
        LoadProcessesConfigCommand = new RelayCommand(async () => await LoadProcessesConfigAsync());
        AddProcessCommand = new RelayCommand(AddProcess);
        RemoveProcessCommand = new RelayCommand(RemoveProcess, () => SelectedProcess != null);
        AddOperationCommand = new RelayCommand(AddOperation, () => SelectedProcess != null);
        RemoveOperationCommand = new RelayCommand(RemoveOperation, () => SelectedProcess != null);
    }

    private async Task SaveVmConfigAsync()
    {
        VmConfig.Devices = [.. AvailableDevices.Where(d => d.IsSelected).Select(d => d.MapToDeviceConfig())];

        var dialog = new SaveFileDialog { Filter = _vmConfigRepo.FileFilter };
        dialog.ShowDialog();
        string filePath = dialog.FileName;

        if (!string.IsNullOrEmpty(filePath))
            await _vmConfigRepo.SaveAsync(VmConfig, filePath);
    }

    private async Task LoadVmConfigAsync()
    {
        var dialog = new OpenFileDialog { Filter = _vmConfigRepo.FileFilter };
        bool? result = dialog.ShowDialog();
        string filePath = dialog.FileName;

        if (string.IsNullOrEmpty(filePath))
            return;

        var config = await _vmConfigRepo.LoadAsync(filePath);
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

    private async Task SaveProcessesConfigAsync()
    {
        var dialog = new SaveFileDialog { Filter = _vmConfigRepo.FileFilter };
        dialog.ShowDialog();
        string filePath = dialog.FileName;
        if (!string.IsNullOrEmpty(filePath))
            await _processConfigRepo.SaveAsync(Processes.ToList(), filePath);
    }

    private async Task LoadProcessesConfigAsync()
    {
        var dialog = new OpenFileDialog { Filter = _vmConfigRepo.FileFilter };
        bool? result = dialog.ShowDialog();
        string filePath = dialog.FileName;
        if (string.IsNullOrEmpty(filePath))
            return;

        var processes = await _processConfigRepo.LoadAsync(filePath);
        if (processes is null)
            return;

        Processes.Clear();
        foreach (var process in processes)
            Processes.Add(process);
    }

    private void AddProcess()
    {
        var newProcess = new Process(Processes.Count + 1, $"Processo {Processes.Count + 1}", new List<IOperation>());
        Processes.Add(newProcess);
        SelectedProcess = newProcess;
    }

    private void RemoveProcess()
    {
        if (SelectedProcess != null)
            Processes.Remove(SelectedProcess);
    }

    private void AddOperation()
    {
        if (SelectedProcess is null)
            return;

        // Aqui, podemos adicionar uma operação padrão (por exemplo, uma operação de CPU).
        IOperation newOp = new CpuOperation(3); // Supondo que exista uma classe CpuOperation
        SelectedProcess.Operations.Add(newOp);
        OnPropertyChanged(nameof(SelectedProcess));
    }

    private void RemoveOperation()
    {
        if (SelectedProcess?.Operations?.Count == 0)
            return;

        SelectedProcess.Operations.RemoveAt(SelectedProcess.Operations.Count - 1);
        OnPropertyChanged(nameof(SelectedProcess));
    }
}
