using ProcSim.Core.Enums;
using ProcSim.Core.Models;
using ProcSim.Core.SystemCalls;

namespace ProcSim.Core.Scheduling.Algorithms;

public sealed class FcfsScheduling(ISysCallHandler sysCallHandler) : ISchedulingAlgorithm
{
    public async Task RunAsync(CpuScheduler scheduler, int coreId, Func<CancellationToken, Task> delayFunc, Func<CancellationToken> tokenProvider)
    {
        if (!scheduler.TryDequeueProcess(out Process current))
            return;

        current.GetCurrentOperation().Channel = coreId;

        if (current.State == ProcessState.Ready)
            current.State = ProcessState.Running;

        while (!tokenProvider().IsCancellationRequested && current.State == ProcessState.Running)
        {
            current.AdvanceTick(sysCallHandler);
            await delayFunc(tokenProvider());
        }
    }
}
