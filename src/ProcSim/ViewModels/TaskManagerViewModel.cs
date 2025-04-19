using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Core.Monitoring;
using ProcSim.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;

public sealed partial class TaskManagerViewModel : ObservableObject
{
    private PerformanceMonitor _perfMonitor;
    private readonly double _windowSizeSeconds = 10;

    public ObservableCollection<CoreChartViewModel> CpuCharts { get; } = new();
    public ObservableCollection<IoChartViewModel> IoCharts { get; } = new();

    public void Initialize(PerformanceMonitor perfMonitor)
    {
        if (_perfMonitor != null)
        {
            _perfMonitor.OnCpuUsageUpdated -= PerfMonitor_OnCpuUsageUpdated;
            _perfMonitor.OnIoUsageUpdated -= PerfMonitor_OnIoUsageUpdated;
            _perfMonitor.OnHardwareChanged -= PerfMonitor_OnHardwareChanged;
        }

        _perfMonitor = perfMonitor;
        _perfMonitor.OnCpuUsageUpdated += PerfMonitor_OnCpuUsageUpdated;
        _perfMonitor.OnIoUsageUpdated += PerfMonitor_OnIoUsageUpdated;
        _perfMonitor.OnHardwareChanged += PerfMonitor_OnHardwareChanged;

        RebuildCharts();
    }

    private void PerfMonitor_OnHardwareChanged()
    {
        RebuildCharts();
    }

    private void RebuildCharts()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var coreIds = _perfMonitor.GetCpuCoreIds();
            foreach (var old in CpuCharts.ToList())
                if (!coreIds.Contains(old.CoreId))
                    CpuCharts.Remove(old);

            foreach (int id in coreIds)
                if (CpuCharts.All(c => c.CoreId != id))
                    CpuCharts.Add(new CoreChartViewModel(id, _windowSizeSeconds));

            var devNames = _perfMonitor.GetIoDeviceNames();
            foreach (var old in IoCharts.ToList())
                if (!devNames.Contains(old.DeviceName))
                    IoCharts.Remove(old);
            
            foreach (string name in devNames)
                if (IoCharts.All(d => d.DeviceName != name))
                    IoCharts.Add(new IoChartViewModel(name, _windowSizeSeconds));
        });
    }

    private void PerfMonitor_OnCpuUsageUpdated(Dictionary<int, double> usageByCore)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (CpuCharts.Count == 0 && usageByCore.Count > 0)
                RebuildCharts();

            foreach (var chart in CpuCharts)
            {
                if (usageByCore.TryGetValue(chart.CoreId, out var u))
                    chart.Values.Add(u);
                else
                    chart.Values.Add(0);
            }
        });
    }

    private void PerfMonitor_OnIoUsageUpdated(Dictionary<string, double> usageByDevice)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (IoCharts.Count == 0 && usageByDevice.Count > 0)
                RebuildCharts();

            foreach (var chart in IoCharts)
            {
                if (usageByDevice.TryGetValue(chart.DeviceName, out var u))
                    chart.Values.Add(u);
                else
                    chart.Values.Add(0);
            }
        });
    }
}
