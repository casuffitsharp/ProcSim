using ProcSim.Core.Models;
using ProcSim.Core.Scheduling;

namespace ProcSim.Core;

public class Scheduler
{
    private CancellationTokenSource _cts = new();

    public event Action<Process> ProcessUpdated;
    public event Action TickUpdated;

    public async Task RunAsync(Queue<Process> processes, ISchedulingAlgorithm algorithm, CancellationToken token)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        TaskCompletionSource<bool> tickTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

        var tickTask = Task.Run(async () =>
        {
            try
            {
                while (await timer.WaitForNextTickAsync(_cts.Token))
                {
                    TickUpdated?.Invoke();
                    tickTcs.TrySetResult(true);
                    tickTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                }
            }
            catch (OperationCanceledException) { }
        }, _cts.Token);

        async Task DelayFunc(CancellationToken ct)
        {
            await tickTcs.Task;
        }

        // Executa o algoritmo, injetando a função de delay.
        await algorithm.RunAsync(
            processes,
            process =>
            {
                if (!_cts.Token.IsCancellationRequested)
                    ProcessUpdated?.Invoke(process);
            }, DelayFunc, _cts.Token);

        _cts.Cancel();
        await tickTask;
    }

    public void Cancel() => _cts.Cancel();
}
