namespace ProcSim.Core.New.Monitoring.Models;

public record ProcessIoMetric(DateTime Timestamp, uint DeviceId, uint ChannelId, uint ProcessId, ulong LatencyCycles);
