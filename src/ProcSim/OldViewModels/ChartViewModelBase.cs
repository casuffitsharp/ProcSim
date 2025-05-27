using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ProcSim.ViewModels;

public abstract partial class ChartViewModelBase(double windowSizeSeconds) : ObservableObject
{
    private readonly Dictionary<int, ISeries<double>> _processSeries = [];
    private readonly Dictionary<int, ObservableCollection<double>> _processSeriesValues = [];

    protected ChartViewModelBase(double windowSizeSeconds, string mainSeriesName) : this(windowSizeSeconds)
    {
        SKColor mainColor = SKColors.LightBlue;

        Series =
        [
            new LineSeries<double>
            {
                Name = mainSeriesName,
                Values = MainValues,
                LineSmoothness = 0,
                GeometrySize = 0,
                Stroke = new SolidColorPaint(mainColor) { StrokeThickness = 1 },
                Fill   = new SolidColorPaint(mainColor),
                EnableNullSplitting = false,
                AnimationsSpeed = TimeSpan.Zero,
                IsVisibleAtLegend = false
            }
        ];

        XAxes =
        [
            new Axis
            {
                Name = "Tempo",
                IsVisible = false
            }
        ];

        YAxes =
        [
            new Axis
            {
                MinLimit = 0,
                MaxLimit = 100,
                Labeler = v => $"{v:N0}%",
                ShowSeparatorLines = false,
                CustomSeparators = [0, 100],
                Position = AxisPosition.End,
                TextSize = 12
            }
        ];

        MainValues.CollectionChanged += Values_CollectionChanged;
    }

    public List<ISeries> Series { get; set; }
    public Axis[] XAxes { get; set; }
    public Axis[] YAxes { get; set; }

    [ObservableProperty]
    public partial double XMin { get; set; } = -windowSizeSeconds;

    [ObservableProperty]
    public partial double XMax { get; set; } = 0;

    public Margin Margin { get; set; } = new(70, 10, 50, 10);

    public int CurrentTime
    {
        get => field;
        set
        {
            field = value;
            UpdateMinMax();
        }
    }

    public ObservableCollection<double> MainValues { get; } = [];

    public void AddProcessSeries(int pid)
    {
        if (_processSeries.ContainsKey(pid))
            return;

        ObservableCollection<double> values = [.. Enumerable.Repeat(0.0, MainValues.Count - 1)];

        float hue = ((pid + 1) * 360f / 20) % 360f;
        float satur = 75;
        float light = 50;
        var color = SKColor.FromHsl(hue, satur, light);

        LineSeries<double> series = new()
        {
            Name = $"P{pid}",
            Values = values,
            LineSmoothness = 0,
            GeometrySize = 0,
            Stroke = new SolidColorPaint(color) { StrokeThickness = 2 },
            Fill = null,
            EnableNullSplitting = false,
            AnimationsSpeed = TimeSpan.Zero
        };

        Series.Add(series);
        _processSeries[pid] = series;
        _processSeriesValues[pid] = values;
    }

    public void RemoveProcessSeries(int pid)
    {
        if (!_processSeries.Remove(pid, out var series))
            return;

        _processSeriesValues.Remove(pid);
        Series.Remove(series);
    }

    public void AddValues(Dictionary<int, double> processesValues)
    {
        foreach (var pid in processesValues.Keys.Except(_processSeries.Keys))
            AddProcessSeries(pid);

        foreach ((int pid, var values) in _processSeriesValues)
            values.Add(processesValues.TryGetValue(pid, out double usage) ? usage : 0);
    }

    private void Values_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        CurrentTime = MainValues.Count > 0 ? MainValues.Count - 1 : 0;
    }

    private void UpdateMinMax()
    {
        XMax = CurrentTime;
        XMin = XMax - windowSizeSeconds;

        XAxes[0].MinLimit = XMin;
        XAxes[0].MaxLimit = XMax;
    }
}
