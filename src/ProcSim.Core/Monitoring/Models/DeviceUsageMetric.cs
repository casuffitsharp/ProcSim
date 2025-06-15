namespace ProcSim.Core.Monitoring.Models;

public record DeviceUsageMetric
{
    public DeviceUsageMetric() { }

    public DeviceUsageMetric(DateTime timestamp, ulong requestsDelta, ulong busyDelta, ulong cyclesDelta, Dictionary<uint, IoChannelUsageMetric> channelsMetrics)
    {
        Timestamp = timestamp;
        RequestsDelta = requestsDelta;
        BusyDelta = busyDelta;
        ChannelsMetrics = channelsMetrics;
        CyclesDelta = cyclesDelta;
    }

    public DateTime Timestamp { get; set; }
    public ulong RequestsDelta { get; set; }
    public ulong BusyDelta { get; set; }
    public ulong CyclesDelta { get; set; }
    public Dictionary<uint, IoChannelUsageMetric> ChannelsMetrics { get; set; }
}