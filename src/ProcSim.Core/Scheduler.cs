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

        // Cria o PeriodicTimer com intervalo de 1 segundo
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        // Define a função delay que aguarda o próximo tick e notifica via TickUpdated
        async Task delayFunc(CancellationToken ct)
        {
            bool tickOccurred = await timer.WaitForNextTickAsync(ct);
            if (tickOccurred)
            {
                TickUpdated?.Invoke();
            }
        }

        // Injeta a função delay no algoritmo
        await algorithm.RunAsync(processes, process =>
        {
            if (_cts.Token.IsCancellationRequested)
                return;
            ProcessUpdated?.Invoke(process);
        }, delayFunc, _cts.Token);
    }

    public void Cancel()
    {
        _cts.Cancel();
    }
}
