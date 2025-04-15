using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ProcSim.ViewModels;

public abstract partial class ChartViewModelBase(double windowSizeSeconds) : ObservableObject
{
    protected ChartViewModelBase(double windowSizeSeconds, string seriesName) : this(windowSizeSeconds)
    {
        Series =
        [
            new LineSeries<double>
            {
                Name = seriesName,
                Values = Values,
                LineSmoothness = 0,
                GeometrySize = 0,
                Stroke = new SolidColorPaint(SKColors.LightBlue) { StrokeThickness = 1 },
                EnableNullSplitting = false,
                AnimationsSpeed = TimeSpan.FromMilliseconds(0),
            }
        ];

        XAxes =
        [
            new Axis
            {
                Name = "Tempo",
                IsVisible = false,
            }
        ];

        YAxes =
        [
            new Axis
            {
                MinLimit = 0,
                MaxLimit = 100,
                Labeler = value => $"{value:N0}%",
                ShowSeparatorLines = false,
                CustomSeparators = [0, 100],
                Position = AxisPosition.End,
                TextSize = 12,
            }
        ];

        Values.CollectionChanged += Values_CollectionChanged;
    }
    public ISeries[] Series { get; set; }
    public Axis[] XAxes { get; set; }
    public Axis[] YAxes { get; set; }

    [ObservableProperty]
    public partial double XMin { get; set; } = -windowSizeSeconds;

    [ObservableProperty]
    public partial double XMax { get; set; } = 0;

    public Margin Margin { get; set; } = new(0, 10, 50, 10);

    public int CurrentTime
    {
        get => field;
        set
        {
            field = value;
            UpdateMinMax();
        }
    }

    public ObservableCollection<double> Values { get; } = [];

    private void Values_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        CurrentTime = Values.Count > 0 ? Values.Count - 1 : 0;
    }

    private void UpdateMinMax()
    {
        XMax = CurrentTime;
        XMin = XMax - windowSizeSeconds;

        XAxes[0].MinLimit = XMin;
        XAxes[0].MaxLimit = XMax;
    }
}
