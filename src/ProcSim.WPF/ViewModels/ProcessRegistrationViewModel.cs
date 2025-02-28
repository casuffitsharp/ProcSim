using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcSim.Core.Enums;
using ProcSim.Core.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ProcSim.Wpf.ViewModels;

public class ProcessRegistrationViewModel : ObservableObject
{
    private int _nextProcessId = 1;

    public ProcessRegistrationViewModel(ObservableCollection<ProcessViewModel> processes)
    {
        Processes = processes;
        AddProcessCommand = new RelayCommand(AddProcess, CanAddProcess);
        ResetNewProcess();
    }

    public ObservableCollection<ProcessViewModel> Processes { get; }

    public string Name
    {
        get;
        set => SetProperty(ref field, value);
    }

    public int ExecutionTime
    {
        get;
        set => SetProperty(ref field, value);
    }

    public int IoTime
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsIoBound
    {
        get;
        set => SetProperty(ref field, value);
    }

    public IRelayCommand AddProcessCommand { get; }

    private void AddProcess()
    {
        Process process = new(
            id: _nextProcessId++,
            name: Name,
            executionTime: ExecutionTime,
            ioTime: IsIoBound ? IoTime : 0,
            type: IsIoBound ? ProcessType.IoBound : ProcessType.CpuBound
        );

        Processes.Add(new ProcessViewModel(process));
        ResetNewProcess();
    }

    private bool CanAddProcess()
    {
        return !string.IsNullOrWhiteSpace(Name) && ExecutionTime > 0;
    }

    private void ResetNewProcess()
    {
        Name = string.Empty;
        ExecutionTime = 0;
        IoTime = 0;
        IsIoBound = false;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName is nameof(ExecutionTime) or nameof(Name))
        {
            AddProcessCommand.NotifyCanExecuteChanged();
        }
    }
}
