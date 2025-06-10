namespace ProcSim.Core.Monitoring.Models;

public record ProcessIoMetric(DateTime Timestamp, uint DeviceId, uint ChannelId, int ProcessId, ulong LatencyCycles);
