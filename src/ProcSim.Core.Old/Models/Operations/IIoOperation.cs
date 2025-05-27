using ProcSim.Core.Old.Enums;

namespace ProcSim.Core.Old.Models.Operations;

public interface IIoOperation
{
    IoDeviceType DeviceType { get; }
    string DeviceName { get; set; }
}
