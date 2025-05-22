using System.ComponentModel;

namespace ProcSim.Core.New.IO;

public enum IoDeviceType
{
    [Description("")]
    None,
    [Description("Disco")]
    Disk,
    [Description("USB")]
    USB
}
