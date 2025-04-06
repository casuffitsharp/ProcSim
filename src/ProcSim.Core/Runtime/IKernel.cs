using ProcSim.Core.Models;

namespace ProcSim.Core.Runtime;

public interface IKernel
{
    void RegisterProcess(Process process);
    Task RunAsync(Func<CancellationToken> tokenProvider);
    void UnRegisterProcess(Process process);
}
