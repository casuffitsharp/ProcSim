using ProcSim.Core.Models;
using ProcSim.Core.Enums;

namespace ProcSim.Core.Scheduling.Algorithms;

public sealed class FcfsScheduling : ISchedulingAlgorithm
{
    public async Task RunAsync(Queue<Process> processes, Action<Process> onProcessUpdated, Func<CancellationToken, Task> delayFunc, CancellationToken token)
    {
        // Enquanto houver processos e o token não for cancelado...
        while (processes.Count > 0 && !token.IsCancellationRequested)
        {
            Process process = processes.Dequeue();

            // Se o processo estiver pronto, inicia sua execução.
            if (process.State == ProcessState.Ready)
            {
                process.State = ProcessState.Running;
                onProcessUpdated(process);
            }

            // Executa um tick (para operações de CPU).
            process.ExecuteTick();
            onProcessUpdated(process);

            // Aguarda um tick simulado.
            await delayFunc(token);

            // Se o processo completou todas as operações, não o re-enfileira.
            if (process.State == ProcessState.Completed)
                continue;

            // Se o processo ficou bloqueado (por exemplo, por iniciar uma operação de I/O),
            // ele não é re-enfileirado aqui; o IoManager cuidará disso.
            if (process.State == ProcessState.Blocked)
                continue;

            // Re-enfileira o processo para continuar sua execução.
            processes.Enqueue(process);
        }
    }
}
