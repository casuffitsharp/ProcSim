using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
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
    }

    public int CoreId { get; }

    public ObservableCollection<double> CpuValues { get; } = [];
}
