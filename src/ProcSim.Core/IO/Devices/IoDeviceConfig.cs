using ProcSim.Core.Enums;

namespace ProcSim.Core.IO.Devices;

public record IoDeviceConfig
{
    public string Name { get; init; }
    public IoDeviceType DeviceType { get; init; }
    public int Channels { get; init; }
    public bool IsEnabled { get; set; }
}
