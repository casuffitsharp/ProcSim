using CommunityToolkit.Mvvm.ComponentModel;
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

        Pid = s.Pid;
        Name = s.Name;
        State = s.State;
        CurrentOperation = "";
        Cpu = s.CpuUsage;
        StaticPriority = s.StaticPriority;
        DynamicPriority = s.DynamicPriority;
        IsUserProcess = isUserProcess;
    }

    [ObservableProperty] public partial string Name { get; set; }
    [ObservableProperty] public partial int Pid { get; set; }
    [ObservableProperty] public partial ProcessState State { get; set; }
    [ObservableProperty] public partial string CurrentOperation { get; set; }
    [ObservableProperty] public partial ushort Cpu { get; set; }
    [ObservableProperty] public partial int DynamicPriority { get; set; }

    public bool IsUserProcess { get; private set; }

    public ProcessStaticPriority StaticPriority
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                _simulationController.SetProcessStaticPriority(Pid, value);
                OnPropertyChanged();
            }
        }
    }

    public void UpdateFromSnapshot(ProcessSnapshot s)
    {
        State = s.State;
        CurrentOperation = ""; //TODO
        Cpu = s.CpuUsage;
        DynamicPriority = s.DynamicPriority;
    }
}