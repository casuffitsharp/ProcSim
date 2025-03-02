using ProcSim.Core.Models;
using ProcSim.Core.Scheduling;

namespace ProcSim.Core;

public class Scheduler(TickManager tickManager)
{
    private CancellationTokenSource _cts = new();

    public event Action<Process> ProcessUpdated;

    public async Task RunAsync(Queue<Process> processes, ISchedulingAlgorithm algorithm, CancellationToken token)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        async Task DelayFunc(CancellationToken ct)
        {
            await tickManager.WaitNextTickAsync(ct);
        }

        tickManager.Resume();

        await algorithm.RunAsync(
            processes,
            process =>
            {
                if (!_cts.Token.IsCancellationRequested)
                {
                    ProcessUpdated?.Invoke(process);
                }
            }, DelayFunc, _cts.Token);

        _cts.Cancel();
        tickManager.Pause();
    }

    public void Cancel()
    {
        _cts.Cancel();
    }
}
