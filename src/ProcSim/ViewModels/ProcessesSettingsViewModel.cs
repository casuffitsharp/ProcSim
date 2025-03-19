using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ProcSim.Core.Enums;
using ProcSim.Core.Models;
using ProcSim.Core.Simulation;
using System.Collections.ObjectModel;
using System.ComponentModel;

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

        SaveConfigCommand = new AsyncRelayCommand(SaveConfigAsync);
        LoadConfigCommand = new AsyncRelayCommand(LoadConfigAsync);

        Reset();
    }

    public IRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand AddProcessCommand { get; }
    public IRelayCommand RemoveProcessCommand { get; }

    public IAsyncRelayCommand SaveConfigCommand { get; }
    public IAsyncRelayCommand LoadConfigCommand { get; }

    public ObservableCollection<ProcessViewModel> Processes { get; }

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
        if (index >= 0)
            Processes[index] = SelectedProcess.Commit();
        else
            Processes.Add(SelectedProcess.Commit());

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

    private async Task SaveConfigAsync()
    {
        var dialog = new SaveFileDialog { Filter = _configRepo.FileFilter };
        dialog.ShowDialog();
        string filePath = dialog.FileName;
        if (!string.IsNullOrEmpty(filePath))
            await _configRepo.SaveAsync([.. Processes.Select(p => p.Model)], filePath);
    }

    private async Task LoadConfigAsync()
    {
        var dialog = new OpenFileDialog { Filter = _configRepo.FileFilter };
        bool? result = dialog.ShowDialog();
        string filePath = dialog.FileName;
        if (string.IsNullOrEmpty(filePath))
            return;

        var processes = await _configRepo.LoadAsync(filePath);
        if (processes is null)
            return;

        Processes.Clear();
        foreach (var process in processes)
            Processes.Add(new(process));
    }
}
