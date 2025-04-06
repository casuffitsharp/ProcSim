using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ProcSim.Assets;
using ProcSim.Core.Configuration;
using ProcSim.Core.Enums;
using ProcSim.Core.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace ProcSim.ViewModels;

public partial class ProcessesSettingsViewModel : ObservableObject
{
    private readonly IRepositoryBase<List<Process>> _configRepo;

    public ProcessesSettingsViewModel(IRepositoryBase<List<Process>> configRepo)
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

        LoadConfig(_configRepo.Deserialize(Settings.Default.ProcessesConfig));

        Reset();
    }

    public IRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand AddProcessCommand { get; }
    public IRelayCommand RemoveProcessCommand { get; }

    public IAsyncRelayCommand SaveConfigCommand { get; }
    public IAsyncRelayCommand SaveAsConfigCommand { get; }
    public IAsyncRelayCommand LoadConfigCommand { get; }

    public event Action<ProcessViewModel> ProcessStateChanged;

    public ObservableCollection<ProcessViewModel> Processes { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveConfigCommand))]
    public partial string CurrentFile { get; private set; }

    public List<IoDeviceType> IoDeviceTypes { get; } = [.. Enum.GetValues<IoDeviceType>()];

    public ProcessViewModel SelectedProcess
    {
        get => field;
        set
        {
            ProcessViewModel old = field;
            if (field != null)
                field.PropertyChanged -= SelectedProcess_PropertyChanged;

            if (SetProperty(ref field, value))
            {
                if (field != null)
                    field.PropertyChanged += SelectedProcess_PropertyChanged;

                old?.Reset();
                OnPropertyChanged(nameof(IsProcessSelected));
                SaveCommand.NotifyCanExecuteChanged();
                CancelCommand.NotifyCanExecuteChanged();
                AddProcessCommand.NotifyCanExecuteChanged();
                RemoveProcessCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool IsProcessSelected => SelectedProcess is not null;

    internal void SaveConfig()
    {
        Settings.Default.ProcessesConfig = _configRepo.Serialize(ToProcesses());
    }

    private void SelectedProcess_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ProcessViewModel.IsValid) or nameof(ProcessViewModel.HasChanges))
            SaveCommand.NotifyCanExecuteChanged();
    }

    private void Save()
    {
        if (SelectedProcess is null)
            return;

        int index = Processes.IndexOf(SelectedProcess);
        ProcessViewModel processVm = SelectedProcess.Commit();
        processVm.PropertyChanged += Process_PropertyChanged;
        if (index >= 0)
        {
            Processes[index] = processVm;
        }
        else
        {
            Processes.Add(processVm);
        }

        Reset();
    }

    private void Reset()
    {
        SelectedProcess?.Reset();
        SelectedProcess = null;
    }

    private bool CanSave()
    {
        return SelectedProcess is not null && SelectedProcess.IsValid && SelectedProcess.HasChanges;
    }

    private bool CanCancel()
    {
        return SelectedProcess is not null;
    }

    private void AddProcess()
    {
        Reset();
        SelectedProcess = new(Processes.Select(p => p.Id).DefaultIfEmpty(0).Max() + 1);
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
        await _configRepo.SaveAsync(ToProcesses(), CurrentFile);
    }

    private async Task SaveAsConfigAsync()
    {
        SaveFileDialog dialog = new() { Filter = _configRepo.FileFilter };
        dialog.ShowDialog();
        string filePath = dialog.FileName;
        if (!string.IsNullOrEmpty(filePath))
        {
            await _configRepo.SaveAsync(ToProcesses(), filePath);
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

        List<Process> processes = await _configRepo.LoadAsync(filePath);
        LoadConfig(processes);

        CurrentFile = filePath;
    }

    private void LoadConfig(List<Process> processes)
    {
        if (processes is null)
            return;

        Processes.Clear();
        foreach (Process process in processes)
        {
            ProcessViewModel processVm = new(process);
            Processes.Add(processVm);
            processVm.PropertyChanged += Process_PropertyChanged;
        }
    }

    private List<Process> ToProcesses()
    {
        return [.. Processes.Select(p => p.Model)];
    }

    private void Process_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ProcessViewModel.State))
            ProcessStateChanged?.Invoke(sender as ProcessViewModel);
    }
}
