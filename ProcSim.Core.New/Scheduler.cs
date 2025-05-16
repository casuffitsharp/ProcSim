using System.Collections.Concurrent;
using System.Diagnostics;

namespace ProcSim.Core.New;

public class Scheduler(ConcurrentQueue<PCB> readyQueue, IReadOnlyDictionary<uint, PCB> idlePcbs)
{
    readonly IReadOnlyDictionary<uint, PCB> _idleByCore = idlePcbs;

    public void Admit(PCB pcb)
    {
        Debug.WriteLine($"Admitting process {pcb.ProcessId} to the ready queue.");
        pcb.State = ProcessState.Ready;
        readyQueue.Enqueue(pcb);
    }

    public PCB Preempt(CPU cpu)
    {
        PCB prev = cpu.CurrentPCB;
        if (prev?.State == ProcessState.Running && prev != _idleByCore[cpu.Id])
        {
            prev.State = ProcessState.Ready;
            readyQueue.Enqueue(prev);
        }

        if (readyQueue.TryDequeue(out PCB next))
        {
            Debug.WriteLine($"Picked process {next.ProcessId} from the ready queue for core {cpu.Id}.");
        }
        else
        {
            next = _idleByCore[cpu.Id];
            Debug.WriteLine($"No process in the ready queue for core {cpu.Id}. Picking idle process");
        }

        next.State = ProcessState.Running;
        return next;
    }
}
