using ProcSim.Core.IO;
using ProcSim.Core.Scheduler;

namespace ProcSim.Core.Configuration;

public record VmConfigModel
{
    public List<IoDeviceConfigModel> Devices { get; init; }
    public SchedulerType SchedulerType { get; init; }
    public ushort CpuCores { get; set; }
    public uint Quantum { get; init; }
}
