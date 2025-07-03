using ProcSim.Core.Process;
using System.Diagnostics;

namespace ProcSim.Core.Scheduler;

public sealed class PriorityScheduler(IReadOnlyDictionary<uint, PCB> idlePcbs, Kernel kernel) : IScheduler
{
    private const ulong AGING_QUANTUM = 50;
    private const double ALPHA = 5.0; // IO to CPU time ratio factor
    private const double BETA = 1.0; // Aging factor
    private const double GAMMA = 0.1; // Queue length penalty factor
    private const int MAX_DYNAMIC_PRIORITY = (int)ProcessStaticPriority.RealTime + 4;

    private readonly PriorityQueue<PCB, int> _readyQueue = new();
    private readonly Lock _queueLock = new();

    public void Admit(PCB pcb)
    {
        if (pcb.State == ProcessState.Terminated)
        {
            Debug.WriteLine($"Scheduler (Priority) ignoring attempt to admit terminated process {pcb.ProcessId}.");
            return;
        }

        Debug.WriteLine($"Admitting process {pcb.ProcessId} to the ready queue.");
        pcb.State = ProcessState.Ready;

        int dp = RecalculatePriority(pcb, kernel.GlobalCycle);
        lock (_queueLock)
            _readyQueue.Enqueue(pcb, dp);

        pcb.LastEnqueueCycle = kernel.GlobalCycle;
    }

    public PCB Preempt(CPU cpu)
    {
        PCB prev = cpu.CurrentPCB;
        PCB next = GetNext(cpu.Id);

        if (next == idlePcbs[cpu.Id] && prev?.State == ProcessState.Running)
        {
            Debug.WriteLine($"No process in the ready queue for core {cpu.Id}. Picking same process");
            return prev;
        }

        if (prev?.State == ProcessState.Running && prev != idlePcbs[cpu.Id])
            Admit(prev);

        if (prev == idlePcbs[cpu.Id])
            prev.State = ProcessState.Ready;

        next.State = ProcessState.Running;
        return next;
    }

    public void Decommission(PCB pcb)
    {
        lock (_queueLock)
        {
            var remainingItems = _readyQueue.UnorderedItems
                .Where(item => item.Element.ProcessId != pcb.ProcessId)
                .Select(item => (item.Element, item.Priority));

            _readyQueue.Clear();
            foreach (var (item, priority) in remainingItems)
            {
                _readyQueue.Enqueue(item, priority);
            }
            
            Debug.WriteLine($"Decommissioned process {pcb.ProcessId} from PriorityScheduler.");
        }
    }

    public PCB GetNext(uint cpuId)
    {
        lock (_queueLock)
        {
            while (_readyQueue.TryDequeue(out PCB next, out _))
            {
                if (next.State != ProcessState.Terminated)
                {
                    Debug.WriteLine($"Picked process {next.ProcessId} with priority {next.DynamicPriority} from the ready queue for core {cpuId}.");
                    return next;
                }

                Debug.WriteLine($"Discarding terminated process {next.ProcessId} from ready queue.");
            }
        }

        Debug.WriteLine($"No process in the ready queue for core {cpuId}. Picking idle process");
        return idlePcbs[cpuId];
    }

    private int RecalculatePriority(PCB pcb, ulong now)
    {
        ulong wait = now - pcb.LastEnqueueCycle;
        double boost = BETA * (wait / (double)AGING_QUANTUM);

        double cpuTime = pcb.UserCycles + pcb.SyscallCycles;
        double ioTime = pcb.WaitCycles;
        double total = cpuTime + ioTime + 1;
        double ioCpu = ALPHA * ((ioTime / total) - (cpuTime / total));

        int meanStatic = (int)pcb.StaticPriority;
        int minStatic = meanStatic - 4;
        int maxStatic = meanStatic + 4;
        double raw = meanStatic - boost + ioCpu;

        int previousPriority = pcb.DynamicPriority;

        int higherCount;
        lock (_queueLock)
            higherCount = _readyQueue.UnorderedItems.Count(item => item.Priority >= previousPriority);

        double queuePenalty = GAMMA * higherCount;
        raw += queuePenalty;

        int dynamicPriority = (int)Math.Round(raw);
        dynamicPriority = Math.Clamp(dynamicPriority, minStatic, maxStatic);

        if (previousPriority != dynamicPriority)
            Debug.WriteLine($"[Priority Change] Process {pcb.ProcessId}: Priority changed from {previousPriority} to {dynamicPriority} (Static: {pcb.StaticPriority}, Wait: {wait}, Boost: {boost:F2}, IO: {ioTime}, CPU: {cpuTime}, ioCpu: {ioCpu:F2})");

        pcb.DynamicPriority = dynamicPriority;

        int queueKey = MAX_DYNAMIC_PRIORITY - dynamicPriority;
        return queueKey;
    }
}
