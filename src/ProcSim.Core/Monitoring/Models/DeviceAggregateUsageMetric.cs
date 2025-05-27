namespace ProcSim.Core.Monitoring.Models;

// I/O agregado por dispositivo
public record DeviceAggregateUsageMetric(
    DateTime Timestamp,
    uint DeviceId,
    long RequestsDelta,
    double Utilization
);