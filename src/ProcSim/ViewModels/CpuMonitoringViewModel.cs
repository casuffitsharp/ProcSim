using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Core.Monitoring;
using ProcSim.Core.Monitoring.Models;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace ProcSim.ViewModels;

public class CpuMonitoringViewModel : ObservableObject
{
    private readonly MonitoringService _monitoringService;
    private readonly Dictionary<uint, CoreChartViewModel> _coreChartMap = [];
    private readonly Dispatcher _uiDispatcher;

    public CpuMonitoringViewModel(MonitoringService monitoringService)
    {
        _uiDispatcher = Dispatcher.CurrentDispatcher;
        _monitoringService = monitoringService;
        _monitoringService.OnMetricsUpdated += () => _uiDispatcher.Invoke(OnMetricsUpdated, DispatcherPriority.Background);
        _monitoringService.OnReset += () => _uiDispatcher.Invoke(OnReset, DispatcherPriority.Background);
    }

    public ObservableCollection<CoreChartViewModel> CoreCharts { get; } = [];
    public CoreChartViewModel AggregateChart { get; } = new CoreChartViewModel("Uso Total");

    private void OnMetricsUpdated()
    {
        foreach ((uint coreId, List<CpuUsageMetric> metrics) in _monitoringService.CpuCoreMetrics)
        {
            if (!_coreChartMap.TryGetValue(coreId, out CoreChartViewModel coreChartViewModel))
            {
                coreChartViewModel = new CoreChartViewModel($"CPU {coreId}");
                _coreChartMap[coreId] = coreChartViewModel;
                CoreCharts.Add(coreChartViewModel);
            }

            CpuUsageMetric lastMetric = metrics.LastOrDefault(new CpuUsageMetric());
            coreChartViewModel.AddValue(lastMetric);
        }

        CpuUsageMetric aggregateMetric = _monitoringService.CpuTotalMetrics.LastOrDefault(new CpuUsageMetric());
        AggregateChart.AddValue(aggregateMetric);
    }

    private void OnReset()
    {
        _coreChartMap.Clear();
        CoreCharts.Clear();
        AggregateChart.Reset();
    }
}

