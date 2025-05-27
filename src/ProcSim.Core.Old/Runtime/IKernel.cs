using ProcSim.Core.Old.Models;

namespace ProcSim.Core.Old.Runtime;

public interface IKernel
{
    void RegisterProcess(Process process);
    Task RunAsync(Func<CancellationToken> tokenProvider);
    void UnRegisterProcess(Process process);
}
