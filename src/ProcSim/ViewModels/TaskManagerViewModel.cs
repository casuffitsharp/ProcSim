using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Core.Logging;
using ProcSim.Core.Monitoring;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace ProcSim.ViewModels;

public sealed partial class TaskManagerViewModel : ObservableObject
{
    private readonly IStructuredLogger _logger;
    private readonly DispatcherTimer _windowTimer;
    private readonly PerformanceMonitor _perfMonitor;
    private readonly SimulationDataGenerator _simDataGenerator;

    public TaskManagerViewModel(IStructuredLogger logger)
    {
        _logger = logger;
        //_logger.OnLog += Logger_OnLog;

        SeparateCpuCharts = true;
        NumberOfCores = 4;
        InitializeCpuCharts();
        InitializeIoCharts();

        List<string> ioDevices = [.. IoCharts.Select(x => x.DeviceName)];
        _perfMonitor = new PerformanceMonitor(_logger, NumberOfCores, ioDevices);
        _perfMonitor.OnCpuUsageUpdated += PerfMonitor_OnCpuUsageUpdated;
        _perfMonitor.OnIoUsageUpdated += PerfMonitor_OnIoUsageUpdated;
        _perfMonitor.Start();

        // Para testes, usamos o gerador de dados simulados.
        _simDataGenerator = new SimulationDataGenerator(_logger, NumberOfCores, ioDevices);

        _windowTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _windowTimer.Tick += WindowTimer_Tick;
        _windowTimer.Start();
    }

    public bool SeparateCpuCharts { get; set; }
    public int NumberOfCores { get; set; }

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

    private void WindowTimer_Tick(object sender, EventArgs e)
    {
        //foreach (CoreChartViewModel coreChart in CpuCharts)
        //{
        //    coreChart.CurrentTime++;
        //    coreChart.XMax = coreChart.CurrentTime;
        //    coreChart.XMin = coreChart.XMax - WINDOW_SIZE;
        //}

        //foreach (IoChartViewModel ioChart in IoCharts)
        //{
        //    ioChart.CurrentTime++;
        //    ioChart.XMax = ioChart.CurrentTime;
        //    ioChart.XMin = ioChart.XMax - WINDOW_SIZE;
        //}
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
                        chart.CpuValues.Add(usage);
                }
            }
            else
            {
                double avgUsage = usageByCore.Values.Average();
                CpuCharts[0].CpuValues.Add(avgUsage);
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
                    ioChart.IoValues.Add(usage);
            }
        });
    }

    //private void Logger_OnLog(SimEvent simEvent)
    //{
    //    if (simEvent.EventType.Equals("I/O", StringComparison.OrdinalIgnoreCase))
    //    {
    //        Application.Current.Dispatcher.Invoke(() => DeviceEvents.Add(simEvent));
    //    }
    //    else if (!simEvent.EventType.Equals("CPU", StringComparison.OrdinalIgnoreCase))
    //    {
    //        Application.Current.Dispatcher.Invoke(() => GeneralEvents.Add(simEvent));
    //    }
    //}
}
