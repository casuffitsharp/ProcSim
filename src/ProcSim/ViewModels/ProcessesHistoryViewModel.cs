using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using ProcSim.Converters;
using ProcSim.Core.Monitoring;
using ProcSim.Core.Monitoring.Models;
using ProcSim.Core.Process;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Threading;

namespace ProcSim.ViewModels;

public partial class ProcessesHistoryViewModel : ObservableObject
{
    private const int WINDOW_SIZE_SECONDS = 60;

    private readonly MonitoringService _monitoringService;
    private readonly TaskManagerDetailsViewModel _taskManagerDetails;
    private readonly Dispatcher _uiDispatcher;
    private readonly ObservableCollection<long> _cpuValues = [];
    private readonly ObservableCollection<long> _ioValues = [];
    private readonly ObservableCollection<int> _dynamicPriorityValues = [];
    private readonly ObservableCollection<int> _staticPriorityValues = [];
    private readonly ObservableCollection<int> _stateValues = [];

    private static readonly IList<string> _processStateLabels = [.. Enum.GetValues<ProcessState>().Select(s => EnumDescriptionConverter.GetEnumDescription(s))];

    public ProcessesHistoryViewModel(MonitoringService monitoringService, TaskManagerDetailsViewModel taskManagerDetails)
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
                Position = AxisPosition.End,
                MinStep = 1,
                MinLimit = 0,
                MaxLimit = 55,
                NamePaint = new SolidColorPaint(SKColors.OrangeRed),
                LabelsPaint = new SolidColorPaint(SKColors.OrangeRed),
                NameTextSize = 14,
                TextSize = 12,
            },
            new()
            {
                Name = "Estado",
                Position = AxisPosition.Start,
                Labels = _processStateLabels,
                Labeler = value => _processStateLabels[(int)value],
                MinLimit = 0,
                MaxLimit = 4,
                ShowSeparatorLines = false,
                LabelsPaint = new SolidColorPaint(SKColors.DarkViolet),
                NameTextSize = 0,
                TextSize = 12
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
            new StepLineSeries<int>
            {
                Name = "Prioridade Dinâmica",
                Values = _dynamicPriorityValues,
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.Orange) { StrokeThickness = 3 },
                GeometrySize = 0,
                ScalesYAt = 2
            },
            new StepLineSeries<int>
            {
                Name = "Prioridade Estática",
                Values = _staticPriorityValues,
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.Orange)
                {
                    StrokeThickness = 2,
                    PathEffect = new DashEffect([6, 4])
                },
                GeometrySize = 0,
                ScalesYAt = 2
            },
            new StepLineSeries<int>
            {
                Name = "Estado",
                Values = _stateValues,
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.DarkViolet)
                {
                    StrokeThickness = 2,
                    PathEffect = new DashEffect([6, 4])
                },
                GeometrySize = 0,
                ScalesYAt = 3
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

        ProcessUsageMetric previous = null;
        foreach (ProcessUsageMetric metric in metrics)
        {
            if (previous?.State == ProcessState.Terminated)
                return;

            AddMetric(metric);
            previous = metric;
        }
    }

    private void OnMetricsUpdated()
    {
        if (SelectedProcess is null)
            return;

        ProcessState previousState = (ProcessState)_stateValues.LastOrDefault((int)ProcessState.Running);
        if (previousState == ProcessState.Terminated)
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
        _stateValues.Add((int)metric.State);
    }

    private void Reset()
    {
        _cpuValues.Clear();
        _ioValues.Clear();
        _dynamicPriorityValues.Clear();
        _staticPriorityValues.Clear();
        _stateValues.Clear();
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