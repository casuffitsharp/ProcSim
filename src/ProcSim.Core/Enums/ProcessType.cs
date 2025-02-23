using System.ComponentModel;

namespace ProcSim.Core.Enums;

public enum ProcessType
{
    [Description("CPU")]
    CpuBound,
    [Description("I/O")]
    IoBound
}
