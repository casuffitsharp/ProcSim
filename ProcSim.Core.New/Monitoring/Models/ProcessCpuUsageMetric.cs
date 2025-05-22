namespace ProcSim.Core.New.Monitoring.Models;

// Uso de CPU por processo (cycles/s)
public record ProcessCpuUsageMetric(
    DateTime Timestamp,
    uint ProcessId,
    ulong CyclesDelta
);
