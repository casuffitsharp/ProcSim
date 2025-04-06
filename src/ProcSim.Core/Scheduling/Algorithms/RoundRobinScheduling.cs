using ProcSim.Core.Enums;
using ProcSim.Core.Models;
using ProcSim.Core.SystemCalls;

namespace ProcSim.Core.Scheduling.Algorithms;

public sealed class RoundRobinScheduling(ISysCallHandler sysCallHandler) : PreemptiveAlgorithmBase, ISchedulingAlgorithm
{
    public async Task RunAsync(CpuScheduler scheduler, int coreId, Func<CancellationToken, Task> delayFunc, Func<CancellationToken> tokenProvider)
    {
        if (!scheduler.TryDequeueProcess(out Process current))
            return;

        current.GetCurrentOperation().Channel = coreId;

        if (current.State == ProcessState.Ready)
            current.State = ProcessState.Running;

        uint remainingQuantum = Quantum;

        while (remainingQuantum-- > 0 && !tokenProvider().IsCancellationRequested)
        {
            current.AdvanceTick(sysCallHandler);
            await delayFunc(tokenProvider());

            if (current.State is ProcessState.Blocked or ProcessState.Completed)
                break;
        }

        if (current.State == ProcessState.Running)
            current.State = ProcessState.Ready;

        if (current.State == ProcessState.Ready)
            scheduler.EnqueueProcess(current);
    }
}
