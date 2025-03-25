using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ProcSim.Assets;
using ProcSim.Converters;
using ProcSim.Core.Configuration;
using ProcSim.Core.Enums;
using ProcSim.Core.Factories;
using ProcSim.Core.IO.Devices;
using ProcSim.Core.Scheduling.Algorithms;
using System.Collections.ObjectModel;
using System.IO;

namespace ProcSim.ViewModels;

public partial class VmSettingsViewModel : ObservableObject
{
    private readonly IRepositoryBase<VmConfig> _configRepo;

    public VmSettingsViewModel(IRepositoryBase<VmConfig> configRepo)
    {
        _configRepo = configRepo;

        IoDeviceType[] availableIoDevices = [.. Enum.GetValues<IoDeviceType>().Where(v => v > 0)];
        EnumDescriptionConverter converter = new();
        foreach (IoDeviceType ioDeviceType in availableIoDevices)
        {
            string name = converter.Convert(ioDeviceType, typeof(string), null, null) as string;
            AvailableDevices.Add(new DeviceViewModel { Name = name, DeviceType = ioDeviceType, IsEnabled = false, Channels = 1 });
        }

        SchedulingAlgorithms = [.. Enum.GetValues<SchedulingAlgorithmType>()];

        SaveConfigCommand = new AsyncRelayCommand(SaveConfigToFileAsync, CanSaveConfig);
        SaveAsConfigCommand = new AsyncRelayCommand(SaveAsConfigAsync);
        LoadConfigCommand = new AsyncRelayCommand(LoadConfigFromFileAsync);

        Quantum = 1;
        CanChangeAlgorithm = true;
        
        LoadConfig(_configRepo.Deserialize(Settings.Default.VmConfig));
    }

    public ObservableCollection<DeviceViewModel> AvailableDevices { get; set; } = [];
    public ObservableCollection<SchedulingAlgorithmType> SchedulingAlgorithms { get; set; }

    public IAsyncRelayCommand SaveConfigCommand { get; }
    public IAsyncRelayCommand SaveAsConfigCommand { get; }
    public IAsyncRelayCommand LoadConfigCommand { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveConfigCommand))]
    public partial string CurrentFile { get; private set; }

    public SchedulingAlgorithmType SelectedAlgorithm
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                SelectedAlgorithmInstance = SchedulingAlgorithmFactory.Create(value);

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPreemptive));
            }
        }
    }

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

    internal void SaveConfig()
    {
        Settings.Default.VmConfig = _configRepo.Serialize(ToVmConfig());
    }

    private void UpdateSchedulerQuantum()
    {
        if (SelectedAlgorithmInstance is IPreemptiveAlgorithm preemptiveAlgorithm)
        {
            preemptiveAlgorithm.Quantum = Quantum;
        }
    }

    private bool CanSaveConfig()
    {
        return File.Exists(CurrentFile);
    }

    private async Task SaveConfigToFileAsync()
    {
        await _configRepo.SaveAsync(ToVmConfig(), CurrentFile);
    }

    private async Task SaveAsConfigAsync()
    {
        SaveFileDialog dialog = new() { Filter = _configRepo.FileFilter };
        dialog.ShowDialog();
        string filePath = dialog.FileName;

        if (!string.IsNullOrEmpty(filePath))
        {
            await _configRepo.SaveAsync(ToVmConfig(), filePath);
            CurrentFile = filePath;
        }
    }

    private async Task LoadConfigFromFileAsync()
    {
        OpenFileDialog dialog = new() { Filter = _configRepo.FileFilter };
        dialog.ShowDialog();
        string filePath = dialog.FileName;

        if (string.IsNullOrEmpty(filePath))
            return;

        VmConfig vmConfig = await _configRepo.LoadAsync(filePath);
        LoadConfig(vmConfig);

        CurrentFile = filePath;
    }

    private void LoadConfig(VmConfig vmConfig)
    {
        if (vmConfig is null)
            return;

        foreach (DeviceViewModel deviceVM in AvailableDevices)
        {
            IoDeviceConfig loadedDevice = vmConfig.Devices?.FirstOrDefault(d => d.DeviceType == deviceVM.DeviceType);
            if (loadedDevice is null)
            {
                deviceVM.IsEnabled = false;
                continue;
            }

            deviceVM.UpdateFromDeviceConfig(loadedDevice);
        }
    }

    private VmConfig ToVmConfig()
    {
        return new()
        {
            SchedulingAlgorithmType = SelectedAlgorithm,
            Devices = [.. AvailableDevices.Select(d => d.MapToDeviceConfig())]
        };
    }
}
