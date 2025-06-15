using ProcSim.Core.Monitoring.Models;
using SkiaSharp;

namespace ProcSim.ViewModels;

public class CoreChartViewModel(string name) : ChartViewModelBase(60, name)
{
    private const int SYSTEM_SERIES_ID = 0;
    private const int USER_SERIES_ID = 1;

    public void AddValue(CpuUsageMetric metric)
    {
        double systemUsage = (metric.SyscallCyclesDelta + metric.InterruptCyclesDelta) * 100.0 / metric.CyclesDelta;
        double userUsage = metric.UserCyclesDelta * 100.0 / metric.CyclesDelta;

        AddSeries(USER_SERIES_ID, "User", SKColors.Blue);
        AddValue(USER_SERIES_ID, userUsage);

        AddSeries(SYSTEM_SERIES_ID, "System", SKColors.LightBlue);
        AddValue(SYSTEM_SERIES_ID, systemUsage);
    }
}
