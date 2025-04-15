using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Core.Logging;
using ProcSim.Core.Monitoring;
using System.Collections.ObjectModel;
using System.Windows;

namespace ProcSim.ViewModels;

public sealed partial class TaskManagerViewModel : ObservableObject
{
    private readonly PerformanceMonitor _perfMonitor;

    public TaskManagerViewModel(PerformanceMonitor perfMonitor)
    {
        _perfMonitor = perfMonitor;
        _perfMonitor.OnCpuUsageUpdated += PerfMonitor_OnCpuUsageUpdated;
        _perfMonitor.OnIoUsageUpdated += PerfMonitor_OnIoUsageUpdated;

        SeparateCpuCharts = true;

        List<string> ioDevices = [.. IoCharts.Select(x => x.DeviceName)];

        //_ = new SimulationDataGenerator(_logger, NumberOfCores, ioDevices);
    }

    public bool SeparateCpuCharts { get; set; }
    public int NumberOfCores
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                InitializeCpuCharts();
            }
        }
    }

    public ObservableCollection<CoreChartViewModel> CpuCharts { get; } = [];
    public ObservableCollection<IoChartViewModel> IoCharts { get; } = [];
    public ObservableCollection<SimEvent> DeviceEvents { get; } = [];
    public ObservableCollection<SimEvent> GeneralEvents { get; } = [];

    private void InitializeCpuCharts()
    {
        CpuCharts.Clear();
        int numberOfCharts = SeparateCpuCharts ? NumberOfCores : 1;
        for (int i = 0; i < numberOfCharts; i++)
            CpuCharts.Add(new CoreChartViewModel(i, 10));
    }

    private void InitializeIoCharts()
    {
        IoCharts.Clear();
        IoCharts.Add(new IoChartViewModel("Disk", 10));
        IoCharts.Add(new IoChartViewModel("Memory", 10));
    }

    private void PerfMonitor_OnCpuUsageUpdated(Dictionary<int, double> usageByCore)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (SeparateCpuCharts && NumberOfCores > 1)
            {
                foreach (CoreChartViewModel chart in CpuCharts)
                {
                    if (usageByCore.TryGetValue(chart.CoreId, out double usage))
                        chart.Values.Add(usage);
                }
            }
            else
            {
                double avgUsage = usageByCore.Values.Average();
                CpuCharts[0].Values.Add(avgUsage);
            }
        });
    }

    private void PerfMonitor_OnIoUsageUpdated(Dictionary<string, double> usageByDevice)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            foreach (IoChartViewModel ioChart in IoCharts)
            {
                if (usageByDevice.TryGetValue(ioChart.DeviceName, out double usage))
                    ioChart.Values.Add(usage);
            }
        });
    }
}
