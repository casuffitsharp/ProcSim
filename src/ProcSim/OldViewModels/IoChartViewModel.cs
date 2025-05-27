namespace ProcSim.ViewModels;

public partial class IoChartViewModel(string deviceName, double windowSizeSeconds) : ChartViewModelBase(windowSizeSeconds, deviceName)
{
    public string DeviceName { get; } = deviceName;
}
