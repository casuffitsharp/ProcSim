using ProcSim.Core.New.Scheduler;

namespace ProcSim.Core.New.Configuration;

public record VmConfig
{
    public List<IoDeviceConfig> Devices { get; init; }
    public SchedulerType SchedulerType { get; init; }
    public ushort CpuCores { get; set; }
    public uint Quantum { get; init; }
}
