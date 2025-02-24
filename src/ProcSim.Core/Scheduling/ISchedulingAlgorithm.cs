using ProcSim.Core.Models;

namespace ProcSim.Core.Scheduling;

public interface ISchedulingAlgorithm
{
    Task RunAsync(Queue<Process> processes, Action<Process> onProcessUpdated, CancellationToken token);
}
