using ProcSim.Core.Old.Models;
using ProcSim.Core.Old.Models.Operations;

namespace ProcSim.Core.Old.SystemCalls;

public interface ISysCallHandler
{
    void RequestIo(Process process, IoOperation operation);
}
