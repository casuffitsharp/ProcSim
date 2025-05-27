using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ProcSim.Assets;
using ProcSim.Core.Configuration;
using ProcSim.Core.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace ProcSim.ViewModels;

public partial class ProcessesConfigViewModel : ObservableObject
{
    private readonly IRepositoryBase<List<ProcessConfigModel>> _configRepo;

    public ProcessesConfigViewModel(IRepositoryBase<List<ProcessConfigModel>> configRepo)
    {
        _configRepo = configRepo;

        SaveCommand = new RelayCommand(Save, CanSave);
        CancelCommand = new RelayCommand(Reset, CanCancel);
        AddProcessCommand = new RelayCommand(AddProcess, CanAddProcess);
        RemoveProcessCommand = new RelayCommand(RemoveProcess, CanRemoveProcess);

        SaveConfigCommand = new AsyncRelayCommand(SaveConfigToFileAsync, CanSaveConfig);
        SaveAsConfigCommand = new AsyncRelayCommand(SaveAsConfigAsync);
        LoadConfigCommand = new AsyncRelayCommand(LoadConfigFromFileAsync);

        SaveCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
        AddProcessCommand.NotifyCanExecuteChanged();
        RemoveProcessCommand.NotifyCanExecuteChanged();

        CanChangeConfigs = true;

        UpdateFromModel(_configRepo.Deserialize(Settings.Default.ProcessesConfig));
        Reset();
    }

    public IRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand AddProcessCommand { get; }
    public IRelayCommand RemoveProcessCommand { get; }

    public IAsyncRelayCommand SaveConfigCommand { get; }
    public IAsyncRelayCommand SaveAsConfigCommand { get; }
    public IAsyncRelayCommand LoadConfigCommand { get; }

    public ObservableCollection<ProcessConfigViewModel> Processes { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveConfigCommand))]
    public partial string CurrentFile { get; private set; }

    [ObservableProperty]
    public partial bool CanChangeConfigs { get; set; }

    // falta verificar como será tratado dispositivo que não está ativo
    public List<IoDeviceType> IoDeviceTypes { get; } = [.. Enum.GetValues<IoDeviceType>()];

    public ProcessConfigViewModel SelectedProcess
    {
        get => field;
        set
        {
            ProcessConfigViewModel old = field;
            if (field != null)
                field.PropertyChanged -= SelectedProcess_PropertyChanged;

            SelectedProcessRef = value;
            value = value?.Copy();

            if (SetProperty(ref field, value))
            {
                if (field != null)
                    field.PropertyChanged += SelectedProcess_PropertyChanged;

                OnPropertyChanged(nameof(IsProcessSelected));
                SaveCommand.NotifyCanExecuteChanged();
                CancelCommand.NotifyCanExecuteChanged();
                AddProcessCommand.NotifyCanExecuteChanged();
                RemoveProcessCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private ProcessConfigViewModel SelectedProcessRef { get; set; }

    public bool IsProcessSelected => SelectedProcess is not null;

    internal void SaveConfig()
    {
        Settings.Default.ProcessesConfig = _configRepo.Serialize(MapToModel());
    }

    private List<ProcessConfigModel> MapToModel()
    {
        return [.. Processes.Select(p => p.MapToModel())];
    }

    private void SelectedProcess_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        //if (e.PropertyName is nameof(ProcessConfigViewModel.IsValid))
        SaveCommand.NotifyCanExecuteChanged();
    }

    private void Save()
    {
        if (SelectedProcess is null)
            return;

        int index = Processes.IndexOf(SelectedProcessRef);
        if (index >= 0)
        {
            Processes[index] = SelectedProcess;
        }
        else
        {
            Processes.Add(SelectedProcess);
        }

        Reset();
    }

    private void Reset()
    {
        SelectedProcess = null;
    }

    private bool CanSave()
    {
        return SelectedProcess is not null && SelectedProcess.IsValid && !SelectedProcess.Equals(SelectedProcessRef);
    }

    private bool CanCancel()
    {
        return SelectedProcess is not null;
    }

    private void AddProcess()
    {
        Reset();
        SelectedProcess = new();
    }

    private void RemoveProcess()
    {
        Processes.Remove(SelectedProcess);
        Reset();
    }

    private bool CanAddProcess()
    {
        return SelectedProcess is null;
    }

    private bool CanRemoveProcess()
    {
        return SelectedProcess is not null && Processes.Contains(SelectedProcess);
    }

    private bool CanSaveConfig()
    {
        return Processes.Count > 0 && File.Exists(CurrentFile);
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
        bool? result = dialog.ShowDialog();
        string filePath = dialog.FileName;
        if (string.IsNullOrEmpty(filePath))
            return;

        List<ProcessConfigModel> processes = await _configRepo.LoadAsync(filePath);
        UpdateFromModel(processes);
        CurrentFile = filePath;
    }

    private void UpdateFromModel(List<ProcessConfigModel> processes)
    {
        Processes.Clear();
        foreach (ProcessConfigModel process in processes)
        {
            ProcessConfigViewModel processVm = new(process);
            Processes.Add(processVm);
        }
    }
}
