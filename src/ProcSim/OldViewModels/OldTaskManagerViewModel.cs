using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Core.Monitoring;
using System.Collections.ObjectModel;
using System.Windows;

namespace ProcSim.ViewModels;

public sealed partial class TaskManagerViewModel : ObservableObject
{
    private PerformanceMonitor _perfMonitor;
    private readonly double _windowSizeSeconds = 10;

    public ObservableCollection<CoreChartViewModel> CpuCharts { get; } = [];
    public ObservableCollection<IoChartViewModel> IoCharts { get; } = [];

    public void Initialize(PerformanceMonitor perfMonitor)
    {
        if (_perfMonitor != null)
        {
            _perfMonitor.OnCpuUsageUpdated -= OnCpu;
            _perfMonitor.OnIoUsageUpdated -= OnIo;
            _perfMonitor.OnHardwareChanged -= OnHardware;
        }

        _perfMonitor = perfMonitor;
        _perfMonitor.OnCpuUsageUpdated += OnCpu;
        _perfMonitor.OnIoUsageUpdated += OnIo;
        _perfMonitor.OnHardwareChanged += OnHardware;
        _perfMonitor.OnProcessUsageByCoreUpdated += OnProc;

        Rebuild();
    }

    private void OnHardware()
    {
        Rebuild();
    }

    private void Rebuild()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var coreIds = _perfMonitor.GetCpuCoreIds();
            foreach (var old in CpuCharts.Where(c => !coreIds.Contains(c.CoreId)).ToList())
                CpuCharts.Remove(old);

            foreach (int id in coreIds.Where(id => !CpuCharts.Any(c => id == c.CoreId)))
                CpuCharts.Add(new CoreChartViewModel(id, _windowSizeSeconds));

            var devNames = _perfMonitor.GetIoDeviceNames();
            foreach (var old in IoCharts.Where(io => !devNames.Contains(io.DeviceName)).ToList())
                IoCharts.Remove(old);

            foreach (string name in devNames.Where(name => !IoCharts.Any(d => d.DeviceName == name)))
                IoCharts.Add(new IoChartViewModel(name, _windowSizeSeconds));
        });
    }

    private void OnCpu(Dictionary<int, double> usageByCore)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (CpuCharts.Count == 0 && usageByCore.Count > 0)
                Rebuild();

            foreach (var chart in CpuCharts)
            {
                if (usageByCore.TryGetValue(chart.CoreId, out var u))
                    chart.MainValues.Add(u);
                else
                    chart.MainValues.Add(0);
            }
        });
    }

    private void OnIo(Dictionary<string, double> usageByDevice)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (IoCharts.Count == 0 && usageByDevice.Count > 0)
                Rebuild();

            foreach (var chart in IoCharts)
            {
                if (usageByDevice.TryGetValue(chart.DeviceName, out var u))
                    chart.MainValues.Add(u);
                else
                    chart.MainValues.Add(0);
            }
        });
    }

    private void OnProc(Dictionary<int, Dictionary<int, double>> usageByProc)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (CpuCharts.Count == 0 && usageByProc.Count > 0)
                Rebuild();

            foreach (var chart in CpuCharts)
            {
                if (!usageByProc.TryGetValue(chart.CoreId, out Dictionary<int, double> coreValues))
                    continue;

                chart.AddValues(coreValues);
            }
        });
    }
}