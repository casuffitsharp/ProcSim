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
    protected readonly Dictionary<uint, ISeries<double>> _series = [];
    protected readonly Dictionary<uint, ObservableCollection<double>> _seriesValues = [];
    private readonly double _windowSizeSeconds;

    protected ChartViewModelBase(double windowSizeSeconds, string title)
    {
        _windowSizeSeconds = windowSizeSeconds;

        XAxes =
        [
            new Axis
            {
                Name = "Tempo",
                IsVisible = false
            }
        ];

        YAxes = [
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
    public Axis[] XAxes { get; private set; }
    public Axis[] YAxes { get; protected set; }

    public string Title { get; }

    public Margin Margin { get; set; } = new(50, 10, 50, 10);

    protected int CurrentTime
    {
        get => field;
        set
        {
            field = value;
            UpdateMinMax();
        }
    }

    protected virtual void AddSeries(uint seriesId, string name, SKColor color = default)
    {
        if (_series.ContainsKey(seriesId))
            return;

        if (color == default)
            color = GenerateColor(seriesId);

        color = color.WithAlpha(150);

        ObservableCollection<double> values = [];
        if (Series.Count == 0)
            values = _mainValues;

        ISeries<double> series = GenerateSeries(name, color, values);

        Series.Add(series);
        _series[seriesId] = series;
        _seriesValues[seriesId] = values;
    }

    protected virtual ISeries<double> GenerateSeries(string name, SKColor color, ObservableCollection<double> values)
    {
        return new LineSeries<double>()
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
        XAxes[0].MinLimit = -_windowSizeSeconds;
        XAxes[0].MaxLimit = 0;
        UpdateMinMax();
    }

    protected static SKColor GenerateColor(uint id)
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
        XAxes[0].MinLimit = CurrentTime - _windowSizeSeconds;
        XAxes[0].MaxLimit = CurrentTime;
    }
}
