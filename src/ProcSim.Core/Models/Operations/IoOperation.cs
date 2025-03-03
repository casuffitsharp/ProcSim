using ProcSim.Core.Enums;

namespace ProcSim.Core.Models.Operations;

public sealed class IoOperation(int duration, IoDeviceType deviceType) : Operation(duration), IIoOperation
{
    public IoDeviceType DeviceType { get; } = deviceType;
}
