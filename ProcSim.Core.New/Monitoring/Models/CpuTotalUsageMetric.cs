namespace ProcSim.Core.New.Monitoring.Models;

// CPU total (agregado)
public record CpuTotalUsageMetric(
    DateTime Timestamp,
    ulong CyclesDelta,
    ulong UserCyclesDelta,
    ulong SyscallCyclesDelta,
    ulong InterruptCyclesDelta
);
