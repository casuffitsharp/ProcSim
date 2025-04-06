namespace ProcSim.Core.Scheduling.Algorithms;

public interface ISchedulingAlgorithm
{
    Task RunAsync(CpuScheduler scheduler, int coreId, Func<CancellationToken, Task> delayFunc, Func<CancellationToken> tokenProvider);
}
