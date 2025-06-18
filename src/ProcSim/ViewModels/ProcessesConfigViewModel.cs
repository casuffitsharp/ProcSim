using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ProcSim.Assets;
using ProcSim.Core.Configuration;
using ProcSim.Core.Extensions;
using ProcSim.Core.Process;
using ProcSim.Core.Simulation;
using ProcSim.Views;
using System.Collections.ObjectModel;
using System.IO;

namespace ProcSim.ViewModels;

public partial class ProcessesConfigViewModel : ObservableObject
{
    private readonly IRepositoryBase<List<ProcessConfigModel>> _configRepo;
    private readonly SimulationController _simulationController;

    public ProcessesConfigViewModel(IRepositoryBase<List<ProcessConfigModel>> configRepo, SimulationController simulationController)
    {
        _configRepo = configRepo;
        _simulationController = simulationController;

        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave, AsyncRelayCommandOptions.None);
        CancelCommand = new RelayCommand(Reset, CanCancel);
        AddProcessCommand = new RelayCommand(AddProcess, CanAddProcess);
        RemoveProcessCommand = new RelayCommand(RemoveProcess, CanRemoveProcess);
        PushProcessCommand = new AsyncRelayCommand<ProcessConfigViewModel>(PushProcessAsync, CanPushProcess, AsyncRelayCommandOptions.None);
        BuildCommand = new AsyncRelayCommand(BuildAsync, CanBuild, AsyncRelayCommandOptions.None);

        SaveConfigCommand = new AsyncRelayCommand(SaveConfigToFileAsync, CanSaveConfig);
        SaveAsConfigCommand = new AsyncRelayCommand(SaveAsConfigAsync);
        LoadConfigCommand = new AsyncRelayCommand(LoadConfigFromFileAsync);

        SaveCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
        AddProcessCommand.NotifyCanExecuteChanged();
        RemoveProcessCommand.NotifyCanExecuteChanged();
        PushProcessCommand.NotifyCanExecuteChanged();
        BuildCommand.NotifyCanExecuteChanged();

        CanChangeConfigs = true;

        List<ProcessConfigModel> model = [];
        if (!string.IsNullOrEmpty(Settings.Default.ProcessesConfig))
            model = _configRepo.Deserialize(Settings.Default.ProcessesConfig);

        UpdateFromModel(model);
        Reset();
    }

    public event Action CurrentFileChanged = () => { };

    public IAsyncRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand AddProcessCommand { get; }
    public IRelayCommand RemoveProcessCommand { get; }
    public IAsyncRelayCommand<ProcessConfigViewModel> PushProcessCommand { get; }
    public IAsyncRelayCommand BuildCommand { get; }

    public IAsyncRelayCommand SaveConfigCommand { get; }
    public IAsyncRelayCommand SaveAsConfigCommand { get; }
    public IAsyncRelayCommand LoadConfigCommand { get; }

    public ObservableCollection<ProcessConfigViewModel> Processes { get; } = [];

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

    [ObservableProperty]
    public partial bool CanChangeConfigs { get; set; }

    public ProcessConfigViewModel SelectedProcess
    {
        get;
        set
        {
            ProcessConfigViewModel old = field;
            SelectedProcessRef = value;
            value = value?.Copy();

            if (SetProperty(ref field, value))
            {
                SaveCommand.NotifyCanExecuteChanged();
                CancelCommand.NotifyCanExecuteChanged();
                AddProcessCommand.NotifyCanExecuteChanged();
                RemoveProcessCommand.NotifyCanExecuteChanged();
                BuildCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private ProcessConfigViewModel SelectedProcessRef { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PushProcessCommand))]
    public partial bool IsSimulationRunning { get; set; }

    internal void SaveConfig()
    {
        Settings.Default.ProcessesConfig = _configRepo.Serialize(MapToModel());
    }

    public List<ProcessConfigModel> MapToModel(Func<ProcessConfigViewModel, bool> filter = null)
    {
        IEnumerable<ProcessConfigViewModel> processes = Processes.AsEnumerable();
        if (filter is not null)
            processes = processes.Where(filter);

        return [.. processes.Select(p => p.MapToModel())];
    }

    private async Task BuildAsync()
    {
        ProcessDto processDto = _simulationController.BuildProgram(SelectedProcessRef.MapToModel(), simulate: true);
        string process = processDto.SerializePretty();
        await TextDialog.ShowTextAsync("Processo compilado", process);
    }

    private async Task SaveAsync()
    {
        if (SelectedProcess is null)
            return;

        List<string> errors = [];
        if (Processes.Any(p => p.Name.Equals(SelectedProcess.Name, StringComparison.InvariantCultureIgnoreCase) && p != SelectedProcessRef))
            errors.Add("Já existe um processo com o mesmo nome.");

        if (!SelectedProcess.Validate(out IEnumerable<string> validationErrors))
            errors.AddRange(validationErrors);

        if (errors.Count > 0)
        {
            await TextDialog.ShowValidationErrorsAsync("Erros de validação", errors);
            return;
        }

        int index = Processes.IndexOf(SelectedProcessRef);
        if (index >= 0)
        {
            SelectedProcessRef.UpdateFromViewModel(SelectedProcess);
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
        return SelectedProcess is not null && !SelectedProcess.Equals(SelectedProcessRef);
    }

    private bool CanCancel()
    {
        return SelectedProcess is not null;
    }

    private bool CanBuild()
    {
        return SelectedProcessRef is not null;
    }


    private void AddProcess()
    {
        Reset();
        SelectedProcess = new();
    }

    private async Task PushProcessAsync(ProcessConfigViewModel process)
    {
        ProcessConfigModel model = process.MapToModel();
        model.Name = $"*{model.Name}";
        _simulationController.RegisterProcess(model);
        await Task.Delay(1000);
    }

    private void RemoveProcess()
    {
        Processes.Remove(SelectedProcessRef);
        Reset();
    }

    private bool CanAddProcess()
    {
        return SelectedProcess is null;
    }

    private bool CanRemoveProcess()
    {
        return SelectedProcessRef is not null && Processes.Contains(SelectedProcessRef);
    }

    private bool CanPushProcess(ProcessConfigViewModel _)
    {
        return IsSimulationRunning == true;
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
