namespace ProcSim.Core.Monitoring.Models;

public record IoChannelUsageMetric(
    ulong RequestsDelta,
    ulong BusyDelta,
    ulong CyclesDelta
);
