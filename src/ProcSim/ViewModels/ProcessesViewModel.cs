using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcSim.Core.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ProcSim.ViewModels;

public partial class ProcessesViewModel : ObservableObject
{
    public ProcessesViewModel(ObservableCollection<ProcessViewModel> processes)
    {
        Processes = processes;
        SaveCommand = new RelayCommand(Save, CanSave);
        CancelCommand = new RelayCommand(Reset, CanCancel);
        AddProcessCommand = new RelayCommand(AddProcess, CanAddProcess);
        RemoveProcessCommand = new RelayCommand(RemoveProcess, CanRemoveProcess);
        Reset();
    }

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

    public IRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand AddProcessCommand { get; }
    public IRelayCommand RemoveProcessCommand { get; }

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
}
