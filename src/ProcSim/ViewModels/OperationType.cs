using System.ComponentModel;

namespace ProcSim.ViewModels;

public enum OperationType
{
    [Description("")]
    None,
    [Description("CPU")]
    Cpu,
    [Description("I/O")]
    Io
}
