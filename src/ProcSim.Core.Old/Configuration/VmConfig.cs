using ProcSim.Core.Old.Enums;
using ProcSim.Core.Old.IO.Devices;

namespace ProcSim.Core.Old.Configuration;

public record VmConfig
{
    public List<IoDeviceConfig> Devices { get; init; }
    public SchedulingAlgorithmType SchedulingAlgorithmType { get; init; }
    public ushort CpuCores { get; set; }
    public uint Quantum { get; init; }
}
