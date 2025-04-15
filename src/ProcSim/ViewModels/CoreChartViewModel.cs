namespace ProcSim.ViewModels;

public partial class CoreChartViewModel(int coreId, double windowSizeSeconds) : ChartViewModelBase(windowSizeSeconds, $"CPU {coreId}")
{
    public int CoreId { get; } = coreId;
}
