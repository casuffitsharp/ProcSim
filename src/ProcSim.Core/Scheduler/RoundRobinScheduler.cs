using ProcSim.Core.Process;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ProcSim.Core.Scheduler;

public sealed class RoundRobinScheduler(IReadOnlyDictionary<uint, PCB> idlePcbs) : IScheduler
{
    private readonly IReadOnlyDictionary<uint, PCB> _idleByCore = idlePcbs;
    private readonly ConcurrentQueue<PCB> readyQueue = new();

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
            Admit(prev);

        if (prev == _idleByCore[cpu.Id])
            prev.State = ProcessState.Ready;

        PCB next = GetNext(cpu.Id);
        next.State = ProcessState.Running;
        return next;
    }

    public PCB GetNext(uint cpuId)
    {
        if (readyQueue.TryDequeue(out PCB next))
        {
            Debug.WriteLine($"Picked process {next.ProcessId} from the ready queue for core {cpuId}.");
        }
        else
        {
            next = _idleByCore[cpuId];
            Debug.WriteLine($"No process in the ready queue for core {cpuId}. Picking idle process");
        }
        return next;
    }
}
