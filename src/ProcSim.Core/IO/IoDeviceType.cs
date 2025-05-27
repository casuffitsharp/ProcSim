using System.ComponentModel;

namespace ProcSim.Core.IO;

public enum IoDeviceType
{
    [Description("")]
    None,
    [Description("Disco")]
    Disk,
    [Description("USB")]
    USB
}
