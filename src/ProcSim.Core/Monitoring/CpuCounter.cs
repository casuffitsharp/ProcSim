namespace ProcSim.Core.Monitoring;

public sealed partial class PerformanceMonitor
{
    internal class CpuCounter
    {
        public DateTime SamplingStart { get; set; }
        public DateTime LastEventTime { get; set; }
        public double RunningTime { get; set; }
        public double TotalTime { get; set; }
        public bool IsRunning { get; set; }
    }
}
