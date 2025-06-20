using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ProcSim.Core.Monitoring.Models;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace ProcSim.ViewModels;

public class DeviceChartViewModel : ChartViewModelBase
{
    public DeviceChartViewModel(string name) : base(60, name)
    {
        YAxes =
        [
            new Axis
            {
                MinLimit = 0,
                Labeler = v => $"{v:N0}%",
                ShowSeparatorLines = true,
                Position = LiveChartsCore.Measure.AxisPosition.End,
                TextSize = 12
            }
        ];
    }

    public void AddValue(DeviceUsageMetric metric)
    {
        uint channelsCount = (uint)metric.ChannelsMetrics.Count;
        foreach ((uint channel, IoChannelUsageMetric channelUsageMetric) in metric.ChannelsMetrics)
        {
            AddSeries(channel, $"Channel {channel}", GenerateColor(channel));
            double usage = (channelUsageMetric.CyclesDelta > 0) ? channelUsageMetric.BusyDelta * 100.0 / (channelUsageMetric.CyclesDelta * channelsCount) : 0;
            _seriesValues[channel].Add(usage);
        }
    }

    protected override ISeries<double> GenerateSeries(string name, SKColor color, ObservableCollection<double> values)
    {
        return new StackedAreaSeries<double>
        {
            Name = name,
            Values = values,
            LineSmoothness = 0,
            GeometrySize = 0,
            Stroke = null,
            Fill = new SolidColorPaint(color),
            AnimationsSpeed = TimeSpan.Zero
        };
    }
}
