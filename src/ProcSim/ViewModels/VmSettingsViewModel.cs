using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ProcSim.Assets;
using ProcSim.Converters;
using ProcSim.Core.Configuration;
using ProcSim.Core.Enums;
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

        AvailableAlgorithms = [.. Enum.GetValues<SchedulingAlgorithmType>().Where(t => t > 0)];

        LoadDevices();

        SaveConfigCommand = new AsyncRelayCommand(SaveConfigToFileAsync, CanSaveConfig);
        SaveAsConfigCommand = new AsyncRelayCommand(SaveAsConfigAsync);
        LoadConfigCommand = new AsyncRelayCommand(LoadConfigFromFileAsync);

        SelectedAlgorithm = AvailableAlgorithms[0];
        Quantum = 1;
        CpuCores = 1;
        CanChangeConfigs = true;

        LoadConfig(_configRepo.Deserialize(Settings.Default.VmConfig));
    }

    public ObservableCollection<SchedulingAlgorithmType> AvailableAlgorithms { get; }
    public ObservableCollection<DeviceViewModel> AvailableDevices { get; private set; }

    public IAsyncRelayCommand SaveConfigCommand { get; }
    public IAsyncRelayCommand SaveAsConfigCommand { get; }
    public IAsyncRelayCommand LoadConfigCommand { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveConfigCommand))]
    public partial string CurrentFile { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPreemptive))]
    public partial SchedulingAlgorithmType SelectedAlgorithm { get; set; }

    [ObservableProperty]
    public partial ushort CpuCores { get; set; }

    [ObservableProperty]
    public partial uint Quantum { get; set; }

    public bool IsPreemptive => SelectedAlgorithm.IsPreemptive();

    [ObservableProperty]
    public partial bool CanChangeConfigs { get; set; }

    internal void SaveConfig()
    {
        Settings.Default.VmConfig = _configRepo.Serialize(ToVmConfig());
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

        SelectedAlgorithm = vmConfig.SchedulingAlgorithmType;
        Quantum = vmConfig.Quantum;
        CpuCores = vmConfig.CpuCores;

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
            Quantum = Quantum,
            CpuCores = CpuCores,
            Devices = [.. AvailableDevices.Select(d => d.MapToDeviceConfig())]
        };
    }

    private void LoadDevices()
    {
        AvailableDevices = [];
        EnumDescriptionConverter converter = new();
        foreach (IoDeviceType type in Enum.GetValues<IoDeviceType>().Where(t => t > 0))
        {
            AvailableDevices.Add(new DeviceViewModel
            {
                DeviceType = type,
                Name = converter.Convert(type, typeof(string)) as string ?? type.ToString(),
                Channels = 1,
                IsEnabled = false
            });
        }
    }
}
