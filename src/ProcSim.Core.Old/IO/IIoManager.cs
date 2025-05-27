using ProcSim.Core.Old.IO.Devices;
using ProcSim.Core.Old.Models;

namespace ProcSim.Core.Old.IO;

public interface IIoManager
{
    event Action<Process> ProcessBecameReady;

    void AddDevice(IIoDevice device);
    void DispatchRequest(IoRequest request);
    void RemoveDevice(string deviceName);
}