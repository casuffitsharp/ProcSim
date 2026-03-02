using ProcSim.Core.Process;
using System.Diagnostics;

namespace ProcSim.Core.Scheduler;

public sealed class RoundRobinScheduler(IReadOnlyDictionary<uint, Pcb> idlePcbs) : IScheduler
{
    private Queue<Pcb> readyQueue = new();
    private readonly Lock _queueLock = new();

    public void Admit(Pcb pcb)
    {
        if (pcb.State == ProcessState.Terminated)
        {
            Debug.WriteLine($"Scheduler (RR) ignoring attempt to admit terminated process {pcb.ProcessId}.");
            return;
        }

        Debug.WriteLine($"Admitting process {pcb.ProcessId} to the ready queue.");
        pcb.State = ProcessState.Ready;
        lock (_queueLock)
            readyQueue.Enqueue(pcb);
    }

    public Pcb Preempt(Cpu cpu)
    {
        Pcb prev = cpu.CurrentPCB;
        if (prev?.State == ProcessState.Running && prev != idlePcbs[cpu.Id])
            Admit(prev);

        if (prev == idlePcbs[cpu.Id])
            prev.State = ProcessState.Ready;

        Pcb next = GetNext(cpu.Id);
        next.State = ProcessState.Running;
        return next;
    }

    public Pcb GetNext(uint cpuId)
    {
        lock (_queueLock)
        {
            while (readyQueue.TryDequeue(out Pcb next))
            {
                if (next.State != ProcessState.Terminated)
                {
                    Debug.WriteLine($"Picked process {next.ProcessId} from the ready queue for core {cpuId}.");
                    return next;
                }
            }
        }

        Debug.WriteLine($"No process in the ready queue for core {cpuId}. Picking idle process");
        return idlePcbs[cpuId];
    }

    public void Decommission(Pcb pcb)
    {
        lock (_queueLock)
        {
            readyQueue = new Queue<Pcb>(readyQueue.Where(p => p.ProcessId != pcb.ProcessId));
            Debug.WriteLine($"Decommissioned process {pcb.ProcessId} from RoundRobinScheduler.");
        }
    }
}