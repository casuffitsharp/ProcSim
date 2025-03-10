using ProcSim.Core.Enums;
using ProcSim.Core.Models;

namespace ProcSim.Core.SystemCalls;

public interface ISysCallHandler
{
    void RequestIo(Process process, int remainingTime, IoDeviceType deviceType);
}
