using ProcSim.Core.Old.Enums;

namespace ProcSim.Core.Old.Models.Operations;

public sealed class IoOperation(int duration, IoDeviceType deviceType) : Operation(duration), IIoOperation
{
    public IoDeviceType DeviceType { get; } = deviceType;

    public string DeviceName { get; set; }
}
