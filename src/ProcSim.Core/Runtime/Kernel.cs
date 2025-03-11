using ProcSim.Core.Enums;
using ProcSim.Core.Models;
using ProcSim.Core.Scheduling;
using ProcSim.Core.Scheduling.Algorithms;
using ProcSim.Core.SystemCalls;

namespace ProcSim.Core.Runtime;

public sealed class Kernel(TickManager tickManager, CpuScheduler cpuScheduler, ISysCallHandler sysCallHandler, ISchedulingAlgorithm schedulingAlgorithm) : IKernel
{
    private readonly CancellationTokenSource _cts = new();
    private readonly Queue<Process> _processTable = new();

    public ISchedulingAlgorithm SchedulingAlgorithm { get; set; } = schedulingAlgorithm;

    public void RegisterProcess(Process process)
    {
        _processTable.Enqueue(process);
        cpuScheduler.EnqueueProcess(process);
    }

    public async Task RunAsync(CancellationToken token)
    {
        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, _cts.Token);

        async Task DelayFunc(CancellationToken ct)
        {
            await tickManager.WaitNextTickAsync(ct);
        }
        
        tickManager.Resume();

        while (!linkedCts.Token.IsCancellationRequested)
        {
            Queue<Process> readyQueue = new();
            while (cpuScheduler.TryDequeueProcess(out Process process))
            {
                if (process.State == ProcessState.Ready)
                    process.State = ProcessState.Running;

                readyQueue.Enqueue(process);
            }

            if (readyQueue.Count == 0)
            {
                await DelayFunc(linkedCts.Token);
                continue;
            }

            Queue<Process> nextReadyQueue = new();
            foreach (Process process in readyQueue)
            {
                process.AdvanceTick(sysCallHandler);

                if (process.State is ProcessState.Blocked or ProcessState.Blocked)
                    continue;

                nextReadyQueue.Enqueue(process);
            }

            // Aplica o algoritmo de escalonamento somente aos processos aptos à CPU.
            await SchedulingAlgorithm.RunAsync(nextReadyQueue, proc => { }, DelayFunc, linkedCts.Token);

            while (nextReadyQueue.Count > 0)
                cpuScheduler.EnqueueProcess(nextReadyQueue.Dequeue());
        }

        tickManager.Pause();
    }

    public void Stop()
    {
        _cts.Cancel();
    }
}
