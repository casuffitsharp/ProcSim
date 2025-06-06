﻿using ProcSim.Core.Models.Operations;
using ProcSim.Core.Old.Enums;
using ProcSim.Core.Old.Models;
using ProcSim.Core.Old.Scheduling;
using ProcSim.Core.Old.SystemCalls;

namespace ProcSim.Core.Old.Scheduling.Algorithms;

public sealed class RoundRobinScheduling(ISysCallHandler sysCallHandler) : PreemptiveAlgorithmBase, ISchedulingAlgorithm
{
    public event Action<int, int?> OnProcessTick;

    public async Task RunAsync(CpuScheduler scheduler, int coreId, Func<CancellationToken, Task> delayFunc, Func<CancellationToken> tokenProvider)
    {
        if (!scheduler.TryDequeueProcess(out Process current))
        {
            OnProcessTick?.Invoke(coreId, null);
            await delayFunc(tokenProvider());
            return;
        }

        if (current.State == ProcessState.Ready)
            current.State = ProcessState.Running;

        uint remainingQuantum = Quantum;

        while (remainingQuantum-- > 0 && !tokenProvider().IsCancellationRequested)
        {
            current.GetCurrentOperation().Channel = coreId;

            OnProcessTick?.Invoke(coreId, current.Id);
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
