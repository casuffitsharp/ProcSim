using ProcSim.Core.Enums;
using ProcSim.Core.Models;

namespace ProcSim.Core.Scheduling;

public class FcfsScheduling : ISchedulingAlgorithm
{
    public async Task RunAsync(Queue<Process> queue, Action<Process> onProcessUpdated, Func<CancellationToken, Task> delayFunc, CancellationToken token)
    {
        while (queue.Count > 0 && !token.IsCancellationRequested)
        {
            if (queue.Count == 0)
            {
                await delayFunc(token);
                continue;
            }

            Process process = queue.Dequeue();

            process.State = ProcessState.Running;
            onProcessUpdated(process);

            // Executa o processo de forma contínua até que ele bloqueie para I/O ou termine.
            while (process.RemainingTime > 0 && !token.IsCancellationRequested && process.State == ProcessState.Running)
            {
                // Regra didática: se o processo precisa de I/O (IoTime > 0) e ainda não o realizou, e se o RemainingTime atingir a metade do ExecutionTime, ele bloqueia para I/O.
                if (!process.IoPerformed && process.IoTime > 0 && process.RemainingTime == process.ExecutionTime / 2)
                {
                    process.State = ProcessState.Blocked;
                    process.IoPerformed = true;
                    onProcessUpdated(process);

                    // Lança uma tarefa que simula a operação de I/O e, ao final, gera a "interrupção"
                    // que reintroduz o processo na fila dos prontos.
                    _ = SimulateIoInterrupt(process, onProcessUpdated, queue, delayFunc, token);
                    break; // Encerra a execução contínua deste processo
                }

                // Executa um tick de CPU.
                await delayFunc(token);
                process.RemainingTime--;
                onProcessUpdated(process);
            }

            if (process.State == ProcessState.Blocked)
                continue;

            if (process.RemainingTime > 0)
            {
                process.State = ProcessState.Ready;
                queue.Enqueue(process);
            }
            else
            {
                process.State = ProcessState.Completed;
            }

            onProcessUpdated(process);
        }
    }

    private static async Task SimulateIoInterrupt(Process process, Action<Process> onProcessUpdated, Queue<Process> queue, Func<CancellationToken, Task> delayFunc, CancellationToken token)
    {
        // Simula o tempo de I/O.
        for (int i = 0; i < process.IoTime && !token.IsCancellationRequested; i++)
            await delayFunc(token);

        // Ao completar o I/O, simula a chegada de uma interrupção:
        process.State = ProcessState.Ready;
        onProcessUpdated(process);

        // Reinsere o processo na fila dos prontos.
        queue.Enqueue(process);
    }
}
