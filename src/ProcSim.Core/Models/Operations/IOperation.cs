using ProcSim.Core.Enums;

namespace ProcSim.Core.Models.Operations;

public interface IOperation
{
    int Duration { get; }
    int RemainingTime { get; }
    bool IsCompleted { get; }
    void ExecuteTick();
}
