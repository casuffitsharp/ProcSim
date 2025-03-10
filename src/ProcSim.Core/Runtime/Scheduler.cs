using ProcSim.Core.Enums;
using ProcSim.Core.Models;
using ProcSim.Core.Scheduling;
using ProcSim.Core.Scheduling.Algorithms;
using ProcSim.Core.SystemCalls;

namespace ProcSim.Core.Runtime;

public class Scheduler(TickManager tickManager, CpuScheduler cpuScheduler, ISysCallHandler sysCallHandler)
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
            Queue<Process> readyQueue = new();
            while (cpuScheduler.TryDequeueProcess(out Process process))
                readyQueue.Enqueue(process);

            if (readyQueue.Count == 0)
            {
                await DelayFunc(linkedCts.Token);
                continue;
            }

            // Fila para os processos que continuarão aptos para CPU.
            Queue<Process> nextReadyQueue = new();

            foreach (Process process in readyQueue)
            {
                // Ao retirar o processo da fila, se ele estiver em Ready, definimos como Running.
                if (process.State == ProcessState.Ready)
                    process.State = ProcessState.Running;

                // O processo avança um tick; se durante esse tick ele invocar a chamada de sistema para I/O,
                // o próprio AdvanceTick atualiza seu estado para Blocked.
                process.AdvanceTick(sysCallHandler);
                ProcessUpdated?.Invoke(process);

                // Se o processo está Completed, não re-adicionamos.
                if (process.State == ProcessState.Completed)
                {
                    ProcessUpdated?.Invoke(process);
                    continue;
                }
                // Se o processo ficou Blocked (solicitou I/O), não re-enfileiramos.
                if (process.State == ProcessState.Blocked)
                    continue;

                // Caso contrário, o processo permanece apto à CPU (e seu estado deve estar Running) e é re-enfileirado.
                if (process.State == ProcessState.Running)
                    nextReadyQueue.Enqueue(process);
            }

            // Executa o algoritmo de escalonamento nos processos aptos à CPU.
            await algorithm.RunAsync(nextReadyQueue, proc => ProcessUpdated?.Invoke(proc), DelayFunc, linkedCts.Token);

            // Reinsere os processos remanescentes no CpuScheduler.
            while (nextReadyQueue.Count > 0)
                cpuScheduler.EnqueueProcess(nextReadyQueue.Dequeue());
        }

        tickManager.Pause();
    }

    public void Cancel()
    {
        _internalCts.Cancel();
    }
}
