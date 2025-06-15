using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Core.IO;
using ProcSim.Core.Monitoring;
using ProcSim.Core.Monitoring.Models;
using ProcSim.Core.Simulation;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace ProcSim.ViewModels;

public class DevicesMonitoringViewModel : ObservableObject
{
    private readonly MonitoringService _monitoringService;
    private readonly SimulationController _simulationController;
    private readonly Dictionary<uint, DeviceChartViewModel> _deviceChartMap = [];
    private readonly Dispatcher _uiDispatcher;

    public DevicesMonitoringViewModel(MonitoringService monitoringService, SimulationController simulationController)
    {
        _uiDispatcher = Dispatcher.CurrentDispatcher;
        _monitoringService = monitoringService;
        _simulationController = simulationController;
        _monitoringService.OnMetricsUpdated += () => _uiDispatcher.Invoke(OnMetricsUpdated, DispatcherPriority.Background);
        _monitoringService.OnReset += () => _uiDispatcher.Invoke(OnReset, DispatcherPriority.Background);
    }

    public ObservableCollection<DeviceChartViewModel> DeviceCharts { get; } = [];

    private void OnMetricsUpdated()
    {
        foreach ((uint deviceId, List<DeviceUsageMetric> metrics) in _monitoringService.DeviceMetrics)
        {
            if (!_deviceChartMap.TryGetValue(deviceId, out DeviceChartViewModel deviceChartViewModel))
            {
                IoDeviceConfigModel deviceConfig = _simulationController.GetDeviceConfig(deviceId);
                deviceChartViewModel = new DeviceChartViewModel(deviceConfig.Name);
                _deviceChartMap[deviceId] = deviceChartViewModel;
                DeviceCharts.Add(deviceChartViewModel);
            }

            DeviceUsageMetric lastMetric = metrics.LastOrDefault(new DeviceUsageMetric());
            deviceChartViewModel.AddValue(lastMetric);
        }
    }

    private void OnReset()
    {
        _deviceChartMap.Clear();
        DeviceCharts.Clear();
    }
}

