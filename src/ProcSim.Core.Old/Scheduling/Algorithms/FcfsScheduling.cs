using ProcSim.Core.Enums;
using ProcSim.Core.Models;
using ProcSim.Core.Models.Operations;
using ProcSim.Core.Old.Runtime;
using ProcSim.Core.Old.SystemCalls;
using System.Collections.Concurrent;

namespace ProcSim.Core.Old.Scheduling.Algorithms;

public sealed class FcfsScheduling(ISysCallHandler sysCallHandler) : ISchedulingAlgorithm
{
    public ConcurrentQueue<PCB> RunQueue { get; }

    public PCB PickNext()
    {
        if (RunQueue.TryDequeue(out PCB pcb))
            return pcb;

        return null;
    }

    //public async Task RunAsync(CpuScheduler scheduler, int coreId, Func<CancellationToken, Task> delayFunc, Func<CancellationToken> tokenProvider)
    //{
    //    if (!scheduler.TryDequeueProcess(out Process current))
    //    {
    //        OnProcessTick?.Invoke(coreId, null);
    //        await delayFunc(tokenProvider());
    //        return;
    //    }

    //    if (current.State == ProcessState.Ready)
    //        current.State = ProcessState.Running;

    //    while (!tokenProvider().IsCancellationRequested && current.State == ProcessState.Running)
    //    {
    //        current.GetCurrentOperation().Channel = coreId;

    //        OnProcessTick?.Invoke(coreId, current.Id);
    //        current.AdvanceTick(sysCallHandler);
    //        await delayFunc(tokenProvider());
    //    }
    //}
}
