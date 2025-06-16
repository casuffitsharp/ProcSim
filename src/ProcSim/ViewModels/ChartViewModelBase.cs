using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ProcSim.ViewModels;

public abstract partial class ChartViewModelBase : ObservableObject
{
    private readonly ObservableCollection<double> _mainValues = [];
    private readonly Dictionary<uint, ISeries<double>> _series = [];
    private readonly Dictionary<uint, ObservableCollection<double>> _seriesValues = [];
    private readonly double _windowSizeSeconds;

    protected ChartViewModelBase(double windowSizeSeconds, string title)
    {
        _windowSizeSeconds = windowSizeSeconds;
        XMin = -_windowSizeSeconds;

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

        Title = title;
        _mainValues.CollectionChanged += Values_CollectionChanged;
    }

    public List<ISeries> Series { get; set; } = [];
    public Axis[] XAxes { get; set; }
    public Axis[] YAxes { get; set; }

    [ObservableProperty]
    public partial double XMin { get; set; }

    [ObservableProperty]
    public partial double XMax { get; set; } = 0;

    public string Title { get; }

    public Margin Margin { get; set; } = new(50, 10, 50, 10);

    public int CurrentTime
    {
        get => field;
        set
        {
            field = value;
            UpdateMinMax();
        }
    }

    public void AddSeries(uint seriesId, string name, SKColor color = default)
    {
        if (_series.ContainsKey(seriesId))
            return;

        if (color == default)
            color = GenerateColor(seriesId);

        color = color.WithAlpha(150);

        ObservableCollection<double> values = [];
        if (Series.Count == 0)
            values = _mainValues;

        LineSeries<double> series = new()
        {
            Name = name,
            Values = values,
            LineSmoothness = 0,
            GeometrySize = 0,
            Stroke = new SolidColorPaint(color) { StrokeThickness = 2 },
            Fill = new SolidColorPaint(color),
            EnableNullSplitting = false,
            AnimationsSpeed = TimeSpan.Zero
        };


        Series.Add(series);
        _series[seriesId] = series;
        _seriesValues[seriesId] = values;
    }

    public void RemoveSeries(uint id)
    {
        if (!_series.Remove(id, out ISeries<double> series))
            return;

        _seriesValues.Remove(id);
        Series.Remove(series);
    }

    public void Reset()
    {
        _mainValues.Clear();
        _series.Clear();
        _seriesValues.Clear();
        Series.Clear();
        CurrentTime = 0;
        XMin = -_windowSizeSeconds;
        XMax = 0;
        UpdateMinMax();
    }

    private static SKColor GenerateColor(uint id)
    {
        float hue = (id + 1) * 360f / 20 % 360f;
        float satur = 75;
        float light = 50;
        return SKColor.FromHsl(hue, satur, light);
    }

    protected void AddValue(uint id, double value)
    {
        ObservableCollection<double> series = _seriesValues[id];
        series.Add(value);
    }

    private void Values_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        CurrentTime = _mainValues.Count > 0 ? _mainValues.Count - 1 : 0;
    }

    private void UpdateMinMax()
    {
        XMax = CurrentTime;
        XMin = XMax - _windowSizeSeconds;

        XAxes[0].MinLimit = XMin;
        XAxes[0].MaxLimit = XMax;
    }
}
