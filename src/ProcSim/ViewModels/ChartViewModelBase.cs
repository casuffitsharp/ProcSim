using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;

namespace ProcSim.ViewModels;

public abstract partial class ChartViewModelBase(double windowSizeSeconds) : ObservableObject
{
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

    private void UpdateMinMax()
    {
        XMax = CurrentTime;
        XMin = XMax - windowSizeSeconds;

        XAxes[0].MinLimit = XMin;
        XAxes[0].MaxLimit = XMax;
    }
}
