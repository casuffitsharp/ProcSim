using ProcSim.Core.Models;
using ProcSim.Core.Enums;

namespace ProcSim.Core.Scheduling.Algorithms;

public sealed class RoundRobinScheduling : PreemptiveAlgorithmBase, ISchedulingAlgorithm
{
    public async Task RunAsync(Queue<Process> processes, Action<Process> onProcessUpdated, Func<CancellationToken, Task> delayFunc, CancellationToken token)
    {
        // Enquanto houver processos na fila e o token não for cancelado...
        while (processes.Count > 0 && !token.IsCancellationRequested)
        {
            Process process = processes.Dequeue();

            // Se o processo estiver pronto, inicia sua execução.
            if (process.State == ProcessState.Ready)
            {
                process.State = ProcessState.Running;
                onProcessUpdated(process);
            }

            int ticksExecuted = 0;

            // Executa até o quantum definido ou até que o processo fique bloqueado ou seja completado.
            while (ticksExecuted < Quantum && !token.IsCancellationRequested && process.State == ProcessState.Running)
            {
                process.ExecuteTick();
                onProcessUpdated(process);
                ticksExecuted++;
                await delayFunc(token);

                // Se o processo inicia uma operação de I/O e muda seu estado para Blocked, sai do loop.
                if (process.State == ProcessState.Blocked)
                    break;
            }

            // Se o processo completou todas as operações, não o re-enfileira.
            if (process.State == ProcessState.Completed)
                continue;

            // Se o processo ficou bloqueado, não o re-enfileira; o IoManager cuidará de reintroduzi-lo.
            if (process.State == ProcessState.Blocked)
                continue;

            // Re-enfileira o processo para a próxima fatia de tempo.
            processes.Enqueue(process);
        }
    }
}
