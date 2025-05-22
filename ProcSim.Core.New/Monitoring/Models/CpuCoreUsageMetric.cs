namespace ProcSim.Core.New.Monitoring.Models;

public record CpuCoreUsageMetric(
    DateTime Timestamp,
    uint CoreId,
    ulong CyclesDelta,
    ulong UserCyclesDelta,
    ulong SyscallCyclesDelta,
    ulong InterruptCyclesDelta
);
