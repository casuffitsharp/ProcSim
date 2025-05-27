using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace ProcSim.ViewModels;

public record DataPoint(DateTime Time, double Value, uint ProcessId);

public class ProcessSeriesViewModel : ObservableObject
{
    public uint ProcessId { get; }
    public ObservableCollection<DataPoint> Points { get; }
    // Color, Label, etc.
}

public class CoreChartViewModel : ObservableObject
{
    public uint CoreId { get; }
    public ObservableCollection<ProcessSeriesViewModel> Series { get; }
}

public class CpuMonitoringViewModel : ObservableObject
{
    public ObservableCollection<CoreChartViewModel> CoreCharts { get; }
    public ProcessSeriesViewModel TotalChart { get; }
}

public class ChannelChartViewModel : ObservableObject
{
    public uint ChannelId { get; }
    public ObservableCollection<ProcessSeriesViewModel> Series { get; }
}

public class DeviceChartViewModel : ObservableObject
{
    public uint DeviceId { get; }
    public ObservableCollection<ChannelChartViewModel> ChannelCharts { get; }
}

