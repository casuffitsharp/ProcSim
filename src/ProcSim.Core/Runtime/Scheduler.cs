using ProcSim.Core.Models;
using ProcSim.Core.Models.Operations;
using ProcSim.Core.Scheduling;
using ProcSim.Core.IO;
using ProcSim.Core.Enums;
using ProcSim.Core.Scheduling.Algorithms;

namespace ProcSim.Core.Runtime;

public class Scheduler(TickManager tickManager, CpuScheduler cpuScheduler, IIoManager ioManager)
{
    private readonly CancellationTokenSource _internalCts = new();

    public event Action<Process> ProcessUpdated;

    public async Task RunAsync(ISchedulingAlgorithm algorithm, CancellationToken token)
    {
        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, _internalCts.Token);

        async Task DelayFunc(CancellationToken ct)
        {
            await tickManager.WaitNextTickAsync(ct);
        }

        tickManager.Resume();

        while (!linkedCts.Token.IsCancellationRequested)
        {
            // Coleta os processos prontos do CpuScheduler
            var readyQueue = new Queue<Process>();
            while (cpuScheduler.TryDequeueProcess(out var process))
                readyQueue.Enqueue(process);

            if (readyQueue.Count > 0)
            {
                // Fila para armazenar os processos que permanecem prontos para CPU ap�s o tick atual.
                var nextReadyQueue = new Queue<Process>();

                foreach (var process in readyQueue)
                {
                    // Obt�m a opera��o atual pendente do processo.
                    var currentOp = process.GetCurrentOperation();
                    if (currentOp is null)
                    {
                        // Se n�o h� mais opera��es, o processo est� conclu�do.
                        ProcessUpdated?.Invoke(process);
                        continue;
                    }

                    // Se a opera��o atual � de I/O, despacha a requisi��o.
                    if (currentOp is IIoOperation ioOperation)
                    {
                        var ioRequest = new IoRequest(process, currentOp.RemainingTime, ioOperation.DeviceType, DateTime.Now);
                        process.State = ProcessState.Blocked;
                        ioManager.DispatchRequest(ioRequest);
                        ProcessUpdated?.Invoke(process);
                    }
                    else if (currentOp is ICpuOperation)
                    {
                        // Se a opera��o � de CPU, executa um tick.
                        process.ExecuteTick();
                        ProcessUpdated?.Invoke(process);
                    }

                    // Ap�s executar o tick (ou despachar I/O), verifica se h� mais opera��es pendentes.
                    var updatedOp = process.GetCurrentOperation();
                    // Se o processo ainda tem uma opera��o ativa e n�o est� bloqueado, re-adiciona � fila.
                    if (updatedOp is not null && process.State != ProcessState.Blocked)
                    {
                        nextReadyQueue.Enqueue(process);
                    }
                }

                // Aplica o algoritmo de escalonamento aos processos prontos para CPU.
                await algorithm.RunAsync(nextReadyQueue, proc => ProcessUpdated?.Invoke(proc), DelayFunc, linkedCts.Token);

                // Re-insere os processos remanescentes no CpuScheduler.
                while (nextReadyQueue.Count > 0)
                {
                    cpuScheduler.EnqueueProcess(nextReadyQueue.Dequeue());
                }
            }
            else
            {
                await DelayFunc(linkedCts.Token);
            }
        }

        tickManager.Pause();
    }

    public void Cancel()
    {
        _internalCts.Cancel();
    }
}
