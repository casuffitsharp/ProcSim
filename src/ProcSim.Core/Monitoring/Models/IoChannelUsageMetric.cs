namespace ProcSim.Core.Monitoring.Models;

// I/O por canal
public record IoChannelUsageMetric(
    DateTime Timestamp,
    uint DeviceId,
    uint ChannelId,
    long RequestsDelta,
    double Utilization
);
