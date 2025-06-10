namespace ProcSim.Core.Monitoring.Models;

public record ProcessCpuUsageMetric(DateTime Timestamp, int ProcessId, ulong CyclesDelta);
