using ProcSim.Core.Enums;

namespace ProcSim.Core.Models.Operations;

public interface IIoOperation
{
    IoDeviceType DeviceType { get; }
}
