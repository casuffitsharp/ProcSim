using ProcSim.Core.Old.IO;
using ProcSim.Core.Old.Models;
using ProcSim.Core.Old.Models.Operations;

namespace ProcSim.Core.Old.SystemCalls;

public sealed class SystemCallHandler(IIoManager ioManager) : ISysCallHandler
{
    public void RequestIo(Process process, IoOperation operation)
    {
        IoRequest ioRequest = new(process, operation, DateTime.Now);
        ioManager.DispatchRequest(ioRequest);
    }
}
