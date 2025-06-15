namespace ProcSim.Core.Monitoring.Models;

public record ProcessUsageMetric(DateTime Timestamp, int ProcessId, double Usage);
