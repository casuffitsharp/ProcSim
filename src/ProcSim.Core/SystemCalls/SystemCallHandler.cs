using ProcSim.Core.IO;
using ProcSim.Core.Models;
using ProcSim.Core.Models.Operations;

namespace ProcSim.Core.SystemCalls;

public sealed class SystemCallHandler(IIoManager ioManager) : ISysCallHandler
{
    public void RequestIo(Process process, IoOperation operation)
    {
        IoRequest ioRequest = new(process, operation, DateTime.Now);
        ioManager.DispatchRequest(ioRequest);
    }
}
