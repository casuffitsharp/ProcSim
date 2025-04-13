namespace ProcSim.Core.Monitoring;

public sealed partial class PerformanceMonitor
{
    internal class IoCounter
    {
        public DateTime SamplingStart { get; set; }
        public DateTime LastEventTime { get; set; }
        public double ActiveTime { get; set; }
        public double TotalTime { get; set; }
        public bool IsActive { get; set; }
    }
}
