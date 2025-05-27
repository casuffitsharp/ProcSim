using System.ComponentModel;

namespace ProcSim.Core.Old.Enums;

public enum ProcessType
{
    [Description("CPU")]
    CpuBound,
    [Description("I/O")]
    IoBound
}
