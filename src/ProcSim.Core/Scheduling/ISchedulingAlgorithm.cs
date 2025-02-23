using ProcSim.Core.Models;

namespace ProcSim.Core.Scheduling;

public interface ISchedulingAlgorithm
{
    bool IsPreemptive { get; }
    Task RunAsync(Queue<Process> processes, Action<Process> onProcessUpdated, CancellationToken token);
}
