using ProcSim.Core.Process;
using System.Diagnostics;

namespace ProcSim.Core.Scheduler;

public sealed class RoundRobinScheduler(IReadOnlyDictionary<uint, PCB> idlePcbs) : IScheduler
{
    private Queue<PCB> readyQueue = new();
    private readonly Lock _queueLock = new();

    public void Admit(PCB pcb)
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

    public PCB Preempt(CPU cpu)
    {
        PCB prev = cpu.CurrentPCB;
        if (prev?.State == ProcessState.Running && prev != idlePcbs[cpu.Id])
            Admit(prev);

        if (prev == idlePcbs[cpu.Id])
            prev.State = ProcessState.Ready;

        PCB next = GetNext(cpu.Id);
        next.State = ProcessState.Running;
        return next;
    }

    public PCB GetNext(uint cpuId)
    {
        lock (_queueLock)
        {
            while (readyQueue.TryDequeue(out PCB next))
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

    public void Decommission(PCB pcb)
    {
        lock (_queueLock)
        {
            readyQueue = new Queue<PCB>(readyQueue.Where(p => p.ProcessId != pcb.ProcessId));
            Debug.WriteLine($"Decommissioned process {pcb.ProcessId} from RoundRobinScheduler.");
        }
    }
}