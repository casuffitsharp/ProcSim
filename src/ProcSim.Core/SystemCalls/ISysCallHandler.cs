using ProcSim.Core.Models;
using ProcSim.Core.Models.Operations;

namespace ProcSim.Core.SystemCalls;

public interface ISysCallHandler
{
    void RequestIo(Process process, IoOperation operation);
}
