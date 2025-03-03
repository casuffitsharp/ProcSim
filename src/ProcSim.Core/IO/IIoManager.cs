using ProcSim.Core.IO.Devices;
using ProcSim.Core.Models;

namespace ProcSim.Core.IO;

public interface IIoManager
{
    event Action<Process> ProcessBecameReady;

    void AddDevice(IIoDevice device);
    void DispatchRequest(IoRequest request);
    void RemoveDevice(string deviceName);
}