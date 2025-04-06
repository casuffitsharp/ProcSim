using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace ProcSim.ViewModels;

public partial class IoChartViewModel : ChartViewModelBase
{
    public IoChartViewModel(string deviceName, double windowSizeSeconds) : base(windowSizeSeconds)
    {
        DeviceName = deviceName;

        Series =
        [
            new LineSeries<double>
            {
                Name = deviceName,
                Values = IoValues,
                LineSmoothness = 0,
                GeometrySize = 0,
                Stroke = new SolidColorPaint(SKColors.LightGreen) { StrokeThickness = 1 },
                EnableNullSplitting = false,
                AnimationsSpeed = TimeSpan.FromMilliseconds(0)
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
                Name = "Atividade",
                MinLimit = 0,
                MaxLimit = 100,
                Labeler = value => $"{value:N0}%",
                ShowSeparatorLines = false,
                Labels = ["0%", "100%"],
                TextSize = 12,
            }
        ];
    }

    public string DeviceName { get; }

    public ObservableCollection<double> IoValues { get; } = [];
}
