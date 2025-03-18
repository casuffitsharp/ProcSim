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
        CancelCommand = new RelayCommand(Cancel, CanCancel);
        Cancel();
    }

    public ObservableCollection<ProcessViewModel> Processes { get; }

    public List<IoDeviceType> IoDeviceTypes { get; } = [.. Enum.GetValues<IoDeviceType>()];

    public ProcessViewModel SelectedProcess
    {
        get => field;
        set
        {
            ProcessViewModel old = field;
            if (SetProperty(ref field, value))
            {
                old?.Reset();
                SaveCommand.NotifyCanExecuteChanged();
                CancelCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public IRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }

    private void Save()
    {
        if (SelectedProcess is null)
            return;

        int index = Processes.IndexOf(SelectedProcess);
        if (index >= 0)
            Processes[index] = SelectedProcess.Commit();
        else
            Processes.Add(SelectedProcess.Commit());

        Cancel();
    }

    private void Cancel()
    {
        SelectedProcess?.Reset();
        SelectedProcess = new(Processes.Select(p => p.Id).DefaultIfEmpty(0).Max() + 1);
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(SelectedProcess?.Name);
    }

    private bool CanCancel()
    {
        return SelectedProcess is not null;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        //if (e.PropertyName is nameof(Name))
        //{
        //    SaveCommand.NotifyCanExecuteChanged();
        //}
    }
}
