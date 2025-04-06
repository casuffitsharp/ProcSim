using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Core.Logging;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace ProcSim.ViewModels;

public partial class TaskManagerViewModel : ObservableObject
{
    private const double WINDOW_SIZE = 10; // janela de 10 segundos
    private readonly IStructuredLogger _logger;
    private readonly DispatcherTimer _timer;
    private readonly Random _random = new();

    public TaskManagerViewModel(IStructuredLogger logger)
    {
        _logger = logger;
        _logger.OnLog += Logger_OnLog;

        SeparateCpuCharts = true;
        NumberOfCores = 4;
        InitializeCpuCharts();
        InitializeIoCharts();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    public bool SeparateCpuCharts
    {
        get => field;
        set
        {
            if (SetProperty(ref field, value))
                InitializeCpuCharts();
        }
    }

    public int NumberOfCores
    {
        get => field;
        set
        {
            if (SetProperty(ref field, value))
                InitializeCpuCharts();
        }
    }

    public ObservableCollection<CoreChartViewModel> CpuCharts { get; } = [];
    public ObservableCollection<IoChartViewModel> IoCharts { get; } = [];
    public ObservableCollection<SimEvent> DeviceEvents { get; } = [];
    public ObservableCollection<SimEvent> GeneralEvents { get; } = [];

    private void InitializeCpuCharts()
    {
        CpuCharts.Clear();
        if (SeparateCpuCharts && NumberOfCores > 1)
        {
            for (int i = 0; i < NumberOfCores; i++)
                CpuCharts.Add(new CoreChartViewModel(i, WINDOW_SIZE));
        }
        else
        {
            CpuCharts.Add(new CoreChartViewModel(0, WINDOW_SIZE));
        }
    }

    private void InitializeIoCharts()
    {
        IoCharts.Clear();
        IoCharts.Add(new IoChartViewModel("Disk", WINDOW_SIZE));
        IoCharts.Add(new IoChartViewModel("Memory", WINDOW_SIZE));
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        foreach (var coreChart in CpuCharts)
        {
            double usage = _random.Next(0, 100);
            coreChart.CpuValues.Add(usage);
            coreChart.CurrentTime++;
        }

        foreach (var ioChart in IoCharts)
        {
            double activity = _random.Next(0, 100);
            ioChart.IoValues.Add(activity);
            ioChart.CurrentTime++;
        }
    }

    private void Logger_OnLog(SimEvent simEvent)
    {
        // Eventos I/O para a coleção de DeviceEvents
        if (simEvent.EventType.Equals("I/O", StringComparison.OrdinalIgnoreCase))
        {
            Application.Current.Dispatcher.Invoke(() => DeviceEvents.Add(simEvent));
        }
        // Eventos não CPU vão para GeneralEvents
        else if (!simEvent.EventType.Equals("CPU", StringComparison.OrdinalIgnoreCase))
        {
            Application.Current.Dispatcher.Invoke(() => GeneralEvents.Add(simEvent));
        }
    }
}
