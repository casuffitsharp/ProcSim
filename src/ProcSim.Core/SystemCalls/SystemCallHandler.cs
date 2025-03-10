using ProcSim.Core.Enums;
using ProcSim.Core.IO;
using ProcSim.Core.Models;

namespace ProcSim.Core.SystemCalls;

public sealed class SystemCallHandler(IIoManager ioManager) : ISysCallHandler
{
    public void RequestIo(Process process, int remainingTime, IoDeviceType deviceType)
    {
        IoRequest ioRequest = new(process, remainingTime, deviceType, DateTime.Now);
        ioManager.DispatchRequest(ioRequest);
    }
}
