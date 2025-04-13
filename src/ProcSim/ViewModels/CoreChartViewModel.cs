using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace ProcSim.ViewModels;

public partial class CoreChartViewModel : ChartViewModelBase
{
    public CoreChartViewModel(int coreId, double windowSizeSeconds) : base(windowSizeSeconds)
    {
        CoreId = coreId;
        Series =
        [
            new LineSeries<double>
            {
                Name = $"CPU {coreId}",
                Values = CpuValues,
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
                Position = LiveChartsCore.Measure.AxisPosition.End,
                TextSize = 12,
            }
        ];

        CpuValues.CollectionChanged += CpuValues_CollectionChanged;
    }

    private void CpuValues_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        CurrentTime = CpuValues.Count > 0 ? CpuValues.Count - 1 : 0;
    }

    public int CoreId { get; }

    public ObservableCollection<double> CpuValues { get; } = [];
}
