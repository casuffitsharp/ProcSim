using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using ProcSim.Core.Monitoring;
using ProcSim.Core.Monitoring.Models;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Threading;

namespace ProcSim.ViewModels;

public partial class ProcessesPriorityHistoryViewModel : ObservableObject
{
    private const int WINDOW_SIZE_SECONDS = 60;

    private readonly MonitoringService _monitoringService;
    private readonly TaskManagerDetailsViewModel _taskManagerDetails;
    private readonly Dispatcher _uiDispatcher;
    private readonly ObservableCollection<long> _cpuValues = [];
    private readonly ObservableCollection<long> _ioValues = [];
    private readonly ObservableCollection<int> _dynamicPriorityValues = [];
    private readonly ObservableCollection<int> _staticPriorityValues = [];

    public ProcessesPriorityHistoryViewModel(MonitoringService monitoringService, TaskManagerDetailsViewModel taskManagerDetails)
    {
        _monitoringService = monitoringService;
        _taskManagerDetails = taskManagerDetails;
        _uiDispatcher = Dispatcher.CurrentDispatcher;
        _cpuValues.CollectionChanged += Values_CollectionChanged;

        XAxes = [new Axis { Name = "Tempo", IsVisible = true }];
        YAxes =
        [
            new()
            {
                Name = "CPU",
                MinLimit = 0,
                IsVisible = false,
                LabelsPaint = new SolidColorPaint(SKColors.Blue),
            },
            new()
            {
                Name = "IO",
                MinLimit = 0,
                IsVisible = false,
                LabelsPaint = new SolidColorPaint(SKColors.Blue),
            },
            new()
            {
                Name = "Prioridade",
                Position = LiveChartsCore.Measure.AxisPosition.End,
                MinLimit = 0,
                MaxLimit = 55,
                NamePaint = new SolidColorPaint(SKColors.OrangeRed),
                LabelsPaint = new SolidColorPaint(SKColors.OrangeRed)
            }
        ];

        ChartSeries =
        [
            new LineSeries<long>
            {
                Name = "CPU",
                Values = _cpuValues,
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 2 },
                LineSmoothness = 0,
                GeometrySize = 0,
                ScalesYAt = 0
            },
            new LineSeries<long>
            {
                Name = "IO",
                Values = _ioValues,
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.Teal) { StrokeThickness = 2 },
                LineSmoothness = 0,
                GeometrySize = 0,
                ScalesYAt = 1
            },
            new LineSeries<int>
            {
                Name = "Prioridade Dinâmica",
                Values = _dynamicPriorityValues,
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.Orange) { StrokeThickness = 3 },
                LineSmoothness = 0,
                GeometrySize = 0,
                ScalesYAt = 2
            },
            new LineSeries<int>
            {
                Name = "Prioridade Estática",
                Values = _staticPriorityValues,
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.Orange)
                {
                    StrokeThickness = 2,
                    PathEffect = new DashEffect([6, 4])
                },
                LineSmoothness = 0,
                GeometrySize = 0,
                ScalesYAt = 2
            }
        ];

        Reset();
        _monitoringService.OnMetricsUpdated += () => _uiDispatcher.Invoke(OnMetricsUpdated, DispatcherPriority.Background);
        ProcessesView = CollectionViewSource.GetDefaultView(_taskManagerDetails.ProcessesDetails);
        ProcessesView.SortDescriptions.Add(new SortDescription(nameof(TaskManagerProcessDetailsViewModel.Name), ListSortDirection.Ascending));
        ProcessesView.Filter = p => p is TaskManagerProcessDetailsViewModel vm && vm.IsUserProcess;
    }

    public ICollectionView ProcessesView { get; }

    [ObservableProperty]
    public partial TaskManagerProcessDetailsViewModel SelectedProcess { get; set; }
    public ObservableCollection<ISeries> ChartSeries { get; set; } = [];
    public Axis[] XAxes { get; set; }
    public Axis[] YAxes { get; set; }

    private int CurrentTime
    {
        get => field;
        set
        {
            field = value;
            UpdateMinMax();
        }
    }

    partial void OnSelectedProcessChanged(TaskManagerProcessDetailsViewModel value)
    {
        if (SelectedProcess is null)
            return;

        Reset();

        if (!_monitoringService.ProcessMetrics.TryGetValue(SelectedProcess.Pid, out List<ProcessUsageMetric> metrics))
            return;

        foreach (ProcessUsageMetric metric in metrics)
            AddMetric(metric);
    }

    private void OnMetricsUpdated()
    {
        if (SelectedProcess is null)
            return;

        if (!_monitoringService.ProcessMetrics.TryGetValue(SelectedProcess.Pid, out List<ProcessUsageMetric> metrics))
            return;

        AddMetric(metrics.LastOrDefault());
    }

    private void AddMetric(ProcessUsageMetric metric)
    {
        if (metric is null)
            return;

        _cpuValues.Add((long)metric.CpuTime);
        _ioValues.Add((long)metric.IoTime);
        _dynamicPriorityValues.Add(metric.DynamicPriority);
        _staticPriorityValues.Add((int)metric.StaticPriority);
    }

    private void Reset()
    {
        _cpuValues.Clear();
        _ioValues.Clear();
        _dynamicPriorityValues.Clear();
        _staticPriorityValues.Clear();
        CurrentTime = 0;
        UpdateMinMax();
    }

    private void Values_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        CurrentTime = _cpuValues.Count > 0 ? _cpuValues.Count - 1 : 0;
    }

    private void UpdateMinMax()
    {
        XAxes[0].MinLimit = CurrentTime - WINDOW_SIZE_SECONDS;
        XAxes[0].MaxLimit = CurrentTime;
    }
}