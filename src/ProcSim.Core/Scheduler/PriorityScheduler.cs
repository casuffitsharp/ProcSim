using ProcSim.Core.Process;
using System.Diagnostics;
using System.Runtime.Intrinsics.Arm;

namespace ProcSim.Core.Scheduler;

public sealed class PriorityScheduler(IReadOnlyDictionary<uint, PCB> idlePcbs, Kernel kernel) : IScheduler
{
    private const ulong AGING_QUANTUM = 50;
    private const double ALPHA = 5.0; // IO to CPU time ratio factor
    private const double BETA = 1.0; // Aging factor
    private const double GAMMA = 0.1; // Queue length penalty factor

    //private static readonly Dictionary<ProcessStaticPriority, (int Min, int Max)> StaticPriorityRanges = new()
    //{
    //    [ProcessStaticPriority.Low] = (1, 10),
    //    [ProcessStaticPriority.BelowNormal] = (11, 20),
    //    [ProcessStaticPriority.Normal] = (21, 30),
    //    [ProcessStaticPriority.AboveNormal] = (31, 40),
    //    [ProcessStaticPriority.High] = (41, 50),
    //    [ProcessStaticPriority.RealTime] = (51, 60),
    //};

    private readonly IReadOnlyDictionary<uint, PCB> _idleByCore = idlePcbs;
    private readonly Kernel _kernel = kernel;
    private readonly PriorityQueue<PCB, int> _readyQueue = new();
    private readonly Lock _queueLock = new();

    public void Admit(PCB pcb)
    {
        Debug.WriteLine($"Admitting process {pcb.ProcessId} to the ready queue.");
        pcb.State = ProcessState.Ready;

        int dp = RecalculatePriority(pcb, _kernel.GlobalCycle);
        lock (_queueLock)
            _readyQueue.Enqueue(pcb, dp);

        pcb.LastEnqueueCycle = _kernel.GlobalCycle;
    }

    public PCB Preempt(CPU cpu)
    {
        PCB prev = cpu.CurrentPCB;
        PCB next = GetNext(cpu.Id);

        if (next == _idleByCore[cpu.Id] && prev?.State == ProcessState.Running)
        {
            Debug.WriteLine($"No process in the ready queue for core {cpu.Id}. Picking same process");
            return prev;
        }

        if (prev?.State == ProcessState.Running && prev != _idleByCore[cpu.Id])
            Admit(prev);

        if (prev == _idleByCore[cpu.Id])
            prev.State = ProcessState.Ready;

        next.State = ProcessState.Running;
        return next;
    }

    public PCB GetNext(uint cpuId)
    {
        lock (_queueLock)
        {
            if (_readyQueue.TryDequeue(out PCB next, out _))
            {
                Debug.WriteLine($"Picked process {next.ProcessId} with priority {next.DynamicPriority} from the ready queue for core {cpuId}.");
                return next;
            }
        }

        Debug.WriteLine($"No process in the ready queue for core {cpuId}. Picking idle process");
        return _idleByCore[cpuId];
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

        int queueKey = minStatic + (maxStatic - dynamicPriority);
        return queueKey;
    }
}
