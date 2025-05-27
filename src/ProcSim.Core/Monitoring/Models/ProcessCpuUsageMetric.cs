namespace ProcSim.Core.Monitoring.Models;

// Uso de CPU por processo (cycles/s)
public record ProcessCpuUsageMetric(
    DateTime Timestamp,
    uint ProcessId,
    ulong CyclesDelta
);
