using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcSim.Core.Enums;
using ProcSim.Core.Models;

namespace ProcSim.Wpf.ViewModels;

public class ProcessRegistrationViewModel : ObservableObject
{
    private int _nextProcessId = 1;

    private string name;
    private int executionTime;
    private int ioTime;
    private bool isIoBound;

    public ProcessRegistrationViewModel(ObservableCollection<ProcessViewModel> processes)
    {
        Processes = processes;
        AddProcessCommand = new RelayCommand(AddProcess, CanAddProcess);
        ResetNewProcess();
    }

    public ObservableCollection<ProcessViewModel> Processes { get; }

    public string Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }

    public int ExecutionTime
    {
        get => executionTime;
        set => SetProperty(ref executionTime, value);
    }

    public int IoTime
    {
        get => ioTime;
        set => SetProperty(ref ioTime, value);
    }

    public bool IsIoBound
    {
        get => isIoBound;
        set => SetProperty(ref isIoBound, value);
    }

    public IRelayCommand AddProcessCommand { get; }

    private void AddProcess()
    {
        var process = new Process(
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
            AddProcessCommand.NotifyCanExecuteChanged();
    }
}
