using ProcSim.Core.Models;

namespace ProcSim.Core.Scheduling.Algorithms;

public interface ISchedulingAlgorithm
{
    Task RunAsync(Queue<Process> processes, Action<Process> onProcessUpdated, Func<CancellationToken, Task> delayFunc, CancellationToken token);
}
