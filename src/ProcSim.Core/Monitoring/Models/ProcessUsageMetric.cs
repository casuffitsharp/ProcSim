using ProcSim.Core.Process;

namespace ProcSim.Core.Monitoring.Models;

public record ProcessUsageMetric
{
    public DateTime Timestamp { get; set; }
    public ulong CpuTime { get; set; }
    public ulong IoTime { get; set; }
    public int DynamicPriority { get; set; }
    public ProcessStaticPriority StaticPriority { get; set; }
    public ProcessState State { get; set; }
}
