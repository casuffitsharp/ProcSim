using ProcSim.Core.Models;
using ProcSim.Core.Scheduling;

namespace ProcSim.Core;

public class Scheduler
{
    private CancellationTokenSource _cts = new();

    public event Action<Process> ProcessUpdated;

    public async Task RunAsync(Queue<Process> processes, ISchedulingAlgorithm algorithm, CancellationToken token)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

        await algorithm.RunAsync(processes, process =>
        {
            if (_cts.Token.IsCancellationRequested)
                return;

            ProcessUpdated?.Invoke(process);
        }, _cts.Token);
    }

    public void Cancel()
    {
        _cts.Cancel();
    }
}
