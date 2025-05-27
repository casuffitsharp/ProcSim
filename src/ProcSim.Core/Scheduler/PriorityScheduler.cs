using ProcSim.Core.Process;
using System.Diagnostics;

namespace ProcSim.Core.Scheduler;

public sealed class PriorityScheduler(IReadOnlyDictionary<uint, PCB> idlePcbs) : IScheduler
{
    private readonly IReadOnlyDictionary<uint, PCB> _idleByCore = idlePcbs;
    private readonly PriorityQueue<PCB, uint> readyQueue = new();
    private readonly Lock _queueLock = new();

    public void Admit(PCB pcb)
    {
        Debug.WriteLine($"Admitting process {pcb.ProcessId} to the ready queue.");
        pcb.State = ProcessState.Ready;
        lock (_queueLock)
            readyQueue.Enqueue(pcb, pcb.Priority);
    }

    public PCB Preempt(CPU cpu)
    {
        PCB prev = cpu.CurrentPCB;
        PCB next = GetNext(cpu.Id);
        
        if (next == _idleByCore[cpu.Id])
        {
            Debug.WriteLine($"No process in the ready queue for core {cpu.Id}. Picking same process");
            return prev;
        }

        if (prev?.State == ProcessState.Running && prev != _idleByCore[cpu.Id])
            Admit(prev);

        next.State = ProcessState.Running;
        return next;
    }

    public PCB GetNext(uint cpuId)
    {
        lock (_queueLock)
        {
            if (readyQueue.TryDequeue(out PCB next, out _))
            {
                Debug.WriteLine($"Picked process {next.ProcessId} with priority {next.Priority} from the ready queue for core {cpuId}.");
                return next;
            }
        }

        Debug.WriteLine($"No process in the ready queue for core {cpuId}. Picking idle process");
        return _idleByCore[cpuId];
    }
}
