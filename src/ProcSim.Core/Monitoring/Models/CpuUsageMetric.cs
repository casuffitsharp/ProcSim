namespace ProcSim.Core.Monitoring.Models;

public record CpuUsageMetric
{
    public CpuUsageMetric() { }

    public CpuUsageMetric(DateTime timestamp, ulong cyclesDelta, ulong userCyclesDelta, ulong syscallCyclesDelta, ulong interruptCyclesDelta, ulong totalIdle)
    {
        Timestamp = timestamp;
        CyclesDelta = cyclesDelta;
        UserCyclesDelta = userCyclesDelta;
        SyscallCyclesDelta = syscallCyclesDelta;
        InterruptCyclesDelta = interruptCyclesDelta;
        TotalIdle = totalIdle;
    }

    public DateTime Timestamp { get; init; }
    public ulong CyclesDelta { get; init; }
    public ulong UserCyclesDelta { get; init; }
    public ulong SyscallCyclesDelta { get; init; }
    public ulong InterruptCyclesDelta { get; init; }
    public ulong TotalIdle { get; init; }
}
