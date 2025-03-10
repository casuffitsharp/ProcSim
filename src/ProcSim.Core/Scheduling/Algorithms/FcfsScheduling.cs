using ProcSim.Core.Models;

namespace ProcSim.Core.Scheduling.Algorithms;

public sealed class FcfsScheduling : ISchedulingAlgorithm
{
    public async Task RunAsync(Queue<Process> processes, System.Action<Process> onProcessUpdated, Func<CancellationToken, Task> delayFunc, CancellationToken token)
    {
        // No FCFS a ordem permanece inalterada; apenas simula um atraso para representar o overhead.
        await delayFunc(token);
    }
}
