using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcSim.Core.Monitoring;
using ProcSim.Core.Process;
using ProcSim.Core.Simulation;

namespace ProcSim.ViewModels;

public partial class TaskManagerProcessDetailsViewModel : ObservableObject
{
    private readonly SimulationController _simulationController;

    public TaskManagerProcessDetailsViewModel(ProcessSnapshot s, SimulationController simulationController, bool isUserProcess)
    {
        _simulationController = simulationController;
        SetPriorityCommand = new RelayCommand<ProcessStaticPriority>(SetPriority, CanSetPriority);

        Pid = s.Pid;
        Name = s.Name;
        State = s.State;
        Cpu = s.CpuUsage;
        StaticPriority = s.StaticPriority;
        DynamicPriority = s.DynamicPriority;
        IsUserProcess = isUserProcess;
    }

    [ObservableProperty] public partial string Name { get; set; }
    [ObservableProperty] public partial int Pid { get; set; }
    [ObservableProperty] public partial ProcessState State { get; set; }
    [ObservableProperty] public partial ushort Cpu { get; set; }
    [ObservableProperty] public partial int DynamicPriority { get; set; }

    public IRelayCommand<ProcessStaticPriority> SetPriorityCommand { get; }

    public bool IsUserProcess { get; private set; }

    [ObservableProperty]
    public partial ProcessStaticPriority StaticPriority { get; set; }

    public void UpdateFromSnapshot(ProcessSnapshot s)
    {
        State = s.State;
        Cpu = s.CpuUsage;
        DynamicPriority = s.DynamicPriority;
    }

    private bool CanSetPriority(ProcessStaticPriority newPriority)
    {
        return IsUserProcess && State != ProcessState.Terminated;
    }

    private void SetPriority(ProcessStaticPriority newPriority)
    {
        if (!CanSetPriority(newPriority))
            return;

        StaticPriority = newPriority;
        _simulationController.SetProcessStaticPriority(Pid, newPriority);
    }
}