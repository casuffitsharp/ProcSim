using ProcSim.Core.Monitoring.Models;
using SkiaSharp;

namespace ProcSim.ViewModels;

public class DeviceChartViewModel(string name) : ChartViewModelBase(60, name)
{
    public void AddValue(DeviceUsageMetric metric)
    {
        foreach ((uint channel, IoChannelUsageMetric channelUsageMetric) in metric.ChannelsMetrics)
        {
            double usage = channelUsageMetric.BusyDelta * 100.0 / channelUsageMetric.CyclesDelta;
            AddSeries(channel, $"Channel {channel}");
            AddValue(channel, usage);
        }
    }
}
