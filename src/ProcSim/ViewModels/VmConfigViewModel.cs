using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ProcSim.Assets;
using ProcSim.Converters;
using ProcSim.Core.Configuration;
using ProcSim.Core.IO;
using ProcSim.Core.Scheduler;
using ProcSim.ViewModels;
using System.Collections.ObjectModel;
using System.IO;

namespace ProcSim.New.ViewModels;

public partial class VmConfigViewModel : ObservableObject
{
    private readonly IRepositoryBase<VmConfigModel> _configRepo;

    public VmConfigViewModel(IRepositoryBase<VmConfigModel> configRepo)
    {
        _configRepo = configRepo;

        SchedulerTypes = [.. Enum.GetValues<SchedulerType>().Where(t => t > 0)];

        SaveConfigCommand = new AsyncRelayCommand(SaveConfigToFileAsync, CanSaveConfig);
        SaveAsConfigCommand = new AsyncRelayCommand(SaveAsConfigAsync);
        LoadConfigCommand = new AsyncRelayCommand(LoadConfigFromFileAsync);

        SchedulerType = SchedulerTypes[0];
        Quantum = 1;
        Cores = 1;
        CanChangeConfigs = true;

        UpdateFromModel(_configRepo.Deserialize(Settings.Default.VmConfig));
    }

    public ushort Cores { get; set; }
    public uint Quantum { get; set; }
    public SchedulerType SchedulerType { get; set; }
    public ObservableCollection<IoDeviceConfigViewModel> Devices { get; set; }

    public ObservableCollection<SchedulerType> SchedulerTypes { get; }

    public IAsyncRelayCommand SaveConfigCommand { get; }
    public IAsyncRelayCommand SaveAsConfigCommand { get; }
    public IAsyncRelayCommand LoadConfigCommand { get; }

    [ObservableProperty]
    public partial bool CanChangeConfigs { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveConfigCommand))]
    public partial string CurrentFile { get; private set; }

    public VmConfigModel MapToModel()
    {
        return new VmConfigModel
        {
            CpuCores = Cores,
            Quantum = Quantum,
            SchedulerType = SchedulerType,
            Devices = [.. Devices.Select(d => d.MapToModel())]
        };
    }

    public void UpdateFromModel(VmConfigModel model)
    {
        Cores = model.CpuCores;
        Quantum = model.Quantum;
        SchedulerType = model.SchedulerType;

        Devices = [];
        EnumDescriptionConverter converter = new();
        foreach (IoDeviceType type in Enum.GetValues<IoDeviceType>().Where(t => t > 0))
        {
            IoDeviceConfigViewModel device = new();

            if (model.Devices?.FirstOrDefault(d => d.Type == type) is IoDeviceConfigModel loadedDevice)
            {
                device.UpdateFromModel(loadedDevice);
            }
            else
            {
                device.Type = type;
                device.Name = converter.Convert(type, typeof(string)) as string ?? type.ToString();
                device.Channels = 1;
                device.IsEnabled = false;
            }

            Devices.Add(device);
        }
    }

    internal void SaveConfig()
    {
        Settings.Default.VmConfig = _configRepo.Serialize(MapToModel());
    }

    private bool CanSaveConfig()
    {
        return File.Exists(CurrentFile);
    }

    private async Task SaveConfigToFileAsync()
    {
        await _configRepo.SaveAsync(MapToModel(), CurrentFile);
    }

    private async Task SaveAsConfigAsync()
    {
        SaveFileDialog dialog = new() { Filter = _configRepo.FileFilter };
        dialog.ShowDialog();
        string filePath = dialog.FileName;

        if (!string.IsNullOrEmpty(filePath))
        {
            await _configRepo.SaveAsync(MapToModel(), filePath);
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

        VmConfigModel vmConfig = await _configRepo.LoadAsync(filePath);
        UpdateFromModel(vmConfig);

        CurrentFile = filePath;
    }
}