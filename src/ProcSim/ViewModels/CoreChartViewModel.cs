using ProcSim.Core.Monitoring.Models;
using SkiaSharp;

namespace ProcSim.ViewModels;

public class CoreChartViewModel(string name) : ChartViewModelBase(60, name)
{
    private const int SYSTEM_SERIES_ID = 0;
    private const int USER_SERIES_ID = 1;

    public void AddValue(CpuUsageMetric metric)
    {
        double systemUsage = 0.0;
        double userUsage = 0.0;
        if (metric.CyclesDelta > 0)
        {
            systemUsage = Math.Min((metric.SyscallCyclesDelta + metric.InterruptCyclesDelta) * 100.0 / metric.CyclesDelta, 100);
            userUsage = Math.Min(metric.UserCyclesDelta * 100.0 / metric.CyclesDelta, 100);
        }

        AddSeries(USER_SERIES_ID, "User", SKColors.Blue);
        AddValue(USER_SERIES_ID, userUsage);

        AddSeries(SYSTEM_SERIES_ID, "System", SKColors.LightBlue);
        AddValue(SYSTEM_SERIES_ID, systemUsage);
    }
}
