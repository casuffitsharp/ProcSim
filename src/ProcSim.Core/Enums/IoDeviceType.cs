using System.ComponentModel;

namespace ProcSim.Core.Enums;

public enum IoDeviceType
{
    [Description("")]
    None,
    [Description("Disco")]
    Disk,
    [Description("Memória")]
    Memory,
    [Description("USB")]
    USB
}
