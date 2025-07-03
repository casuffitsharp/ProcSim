using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ProcSim.Assets;
using ProcSim.Converters;
using ProcSim.Core.Configuration;
using ProcSim.Core.IO;
using ProcSim.Core.Scheduler;
using ProcSim.ViewModels;
using ProcSim.Views;
using System.Collections.ObjectModel;
using System.IO;

namespace ProcSim.New.ViewModels;

public partial class VmConfigViewModel : ObservableObject
{
    private readonly IRepositoryBase<VmConfigModel> _configRepo;

    public VmConfigViewModel(IRepositoryBase<VmConfigModel> configRepo)
    {
        _configRepo = configRepo;

        SaveConfigCommand = new AsyncRelayCommand(SaveConfigToFileAsync, CanSaveConfig);
        SaveAsConfigCommand = new AsyncRelayCommand(SaveAsConfigAsync);
        LoadConfigCommand = new AsyncRelayCommand(LoadConfigFromFileAsync);

        SchedulerType = SchedulerType.Priority;
        Quantum = 10;
        Cores = 1;
        CanChangeConfigs = true;

        VmConfigModel model = null;
        if (!string.IsNullOrEmpty(Settings.Default.VmConfig))
            model = _configRepo.Deserialize(Settings.Default.VmConfig);

        UpdateFromModel(model);
    }

    public static SchedulerType[] SchedulerTypeValues { get; } = [.. Enum.GetValues<SchedulerType>().Where(x => x != SchedulerType.None)];
    public static IoDeviceType[] IoDeviceTypeValues { get; } = [.. Enum.GetValues<IoDeviceType>().Where(x => x != IoDeviceType.None)];

    public event Action CurrentFileChanged = () => { };

    [ObservableProperty]
    public partial ushort Cores { get; set; }
    [ObservableProperty]
    public partial uint Quantum { get; set; }
    [ObservableProperty]
    public partial SchedulerType SchedulerType { get; set; }

    public ObservableCollection<IoDeviceConfigViewModel> IoDevices { get; set; }

    public IAsyncRelayCommand SaveConfigCommand { get; }
    public IAsyncRelayCommand SaveAsConfigCommand { get; }
    public IAsyncRelayCommand LoadConfigCommand { get; }

    [ObservableProperty]
    public partial bool CanChangeConfigs { get; set; }

    public string CurrentFile
    {
        get;
        private set
        {
            if (SetProperty(ref field, value))
            {
                CurrentFileChanged.Invoke();
                SaveConfigCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public VmConfigModel MapToModel()
    {
        return new VmConfigModel
        {
            CpuCores = Cores,
            Quantum = Quantum,
            SchedulerType = SchedulerType,
            Devices = [.. IoDevices.Select(d => d.MapToModel())]
        };
    }

    public void UpdateFromModel(VmConfigModel model)
    {
        if (model is not null)
        {
            Cores = model.CpuCores;
            Quantum = model.Quantum;
            SchedulerType = model.SchedulerType;
        }

        IoDevices ??= [];
        IoDevices.Clear();

        EnumDescriptionConverter converter = new();
        foreach (IoDeviceType type in IoDeviceTypeValues)
        {
            IoDeviceConfigViewModel device = new();

            if (model?.Devices?.FirstOrDefault(d => d.Type == type) is IoDeviceConfigModel loadedDevice)
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

            IoDevices.Add(device);
        }
    }

    public async Task<bool> ValidateAsync()
    {
        List<string> errors = [];

        if (Cores == 0)
            errors.Add("Quantidade de núcleos deve ser maior que 0.");

        if (Quantum == 0)
            errors.Add("Quantum deve ser maior que 0.");

        if (SchedulerType == SchedulerType.None)
            errors.Add("Tipo de escalonador deve ser definido.");

        foreach (IoDeviceConfigViewModel device in IoDevices)
        {
            if (!device.IsEnabled)
                continue;

            if (!device.Validate(out List<string> deviceErrors))
            {
                string type = EnumDescriptionConverter.GetEnumDescription(device.Type);
                errors.AddRange(deviceErrors.Select(e => $"[{type}] {e}"));
            }
        }

        if (errors.Count > 0)
        {
            await TextDialog.ShowValidationErrorsAsync("Erros de validação", errors);
            return false;
        }

        return true;
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
        if (!await ValidateAsync())
            return;

        await _configRepo.SaveAsync(MapToModel(), CurrentFile);
    }

    private async Task SaveAsConfigAsync()
    {
        if (!await ValidateAsync())
            return;

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