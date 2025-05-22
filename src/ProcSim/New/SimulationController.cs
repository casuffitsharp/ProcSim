using ProcSim.Core.New;
using ProcSim.Core.New.Process;
using ProcSim.Core.New.Scheduler;

namespace ProcSim.New;

public class SimulationController
{
    private readonly PeriodicTimer _timer;

    private Kernel _kernel;
    private Action _clockTick = () => { };

    public SimulationController()
    {
        _timer = new PeriodicTimer(Timeout.InfiniteTimeSpan);
        _ = Task.Run(TimerWorker);
    }

    public uint Clock
    {
        get;
        set
        {
            if (value != field)
            {
                field = value;
                if (Status != SimulationStatus.Paused)
                    _timer.Period = TimeSpan.FromMilliseconds(field);
            }
        }
    } = 100;

    public SimulationStatus Status { get; private set; } = SimulationStatus.Stopped;

    public void Resume()
    {
        if (Status == SimulationStatus.Paused)
        {
            Status = SimulationStatus.Running;
            _timer.Period = TimeSpan.FromMilliseconds(Clock);
        }
    }

    public void Pause()
    {
        if (Status == SimulationStatus.Running)
        {
            Status = SimulationStatus.Paused;
            _timer.Period = Timeout.InfiniteTimeSpan;
        }
    }

    public void Start(uint cores, uint quantum, SchedulerType schedulerType)
    {
        _kernel ??= new Kernel();
        _kernel.Initialize(cores, quantum, schedulerType, handler => _clockTick += handler);
        Status = SimulationStatus.Running;
    }

    public void RegisterProcess(ProcessDto process)
    {

    }

    private async Task TimerWorker()
    {
        while (await _timer.WaitForNextTickAsync())
        {
            if (Status == SimulationStatus.Running)
                _clockTick();
        }
    }
}

public enum SimulationStatus
{
    Stopped,
    Running,
    Paused
}