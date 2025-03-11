namespace ProcSim.Core.Models.Operations;

public abstract class Operation(int duration) : IOperation
{
    public int Duration { get; } = duration;
    public int RemainingTime { get; private set; } = duration;

    public bool IsCompleted => RemainingTime <= 0;

    public void ExecuteTick()
    {
        if (RemainingTime > 0)
            RemainingTime--;
    }

    public void Reset()
    {
        RemainingTime = Duration;
    }
}
