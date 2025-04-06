using ProcSim.Core.Enums;
using ProcSim.Core.IO.Devices;

namespace ProcSim.Core.Configuration;

public record VmConfig
{
    public List<IoDeviceConfig> Devices { get; init; }
    public SchedulingAlgorithmType SchedulingAlgorithmType { get; init; }
    public ushort CpuCores { get; set; }
    public uint Quantum { get; init; }
}
