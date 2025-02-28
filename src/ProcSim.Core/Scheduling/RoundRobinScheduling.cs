using ProcSim.Core.Enums;
using ProcSim.Core.Models;

namespace ProcSim.Core.Scheduling;

// Supondo que PreemptiveAlgorithmBase define a propriedade Quantum.
public class RoundRobinScheduling : PreemptiveAlgorithmBase, ISchedulingAlgorithm
{
    public async Task RunAsync(Queue<Process> processes, Action<Process> onProcessUpdated, Func<CancellationToken, Task> delayFunc, CancellationToken token)
    {
        // Converte a fila em uma lista para facilitar inserções e remoções.
        List<Process> readyQueue = [.. processes];

        // Dicionário para saber se o processo já realizou I/O.
        Dictionary<int, bool> ioPerformed = [];
        foreach (Process process in readyQueue)
        {
            ioPerformed[process.Id] = false;
        }

        // Enquanto houver processos prontos.
        while (readyQueue.Count != 0 && !token.IsCancellationRequested)
        {
            // Percorre a fila de processos prontos.
            for (int i = 0; i < readyQueue.Count; i++)
            {
                Process process = readyQueue[i];
                process.State = ProcessState.Running;
                onProcessUpdated(process);

                // O número de ticks a serem executados nesta fatia é o mínimo entre o quantum e o tempo restante.
                int ticksToExecute = Math.Min(Quantum, process.RemainingTime);
                int executedTicks = 0;

                // Executa tick a tick.
                for (int j = 0; j < ticksToExecute && !token.IsCancellationRequested; j++)
                {
                    executedTicks++;

                    // Critério didático de I/O:
                    // Se o processo não realizou I/O, possui IoTime > 0 e,
                    // após executar 'executedTicks', o tempo restante iguala à metade do ExecutionTime,
                    // ele entra em bloqueio para I/O.
                    if (!ioPerformed[process.Id] && process.IoTime > 0 && (process.RemainingTime - executedTicks) == process.ExecutionTime / 2)
                    {
                        process.State = ProcessState.Blocked;
                        onProcessUpdated(process);
                        ioPerformed[process.Id] = true;

                        // Lança a simulação de I/O (simulação de interrupção) de forma assíncrona.
                        _ = SimulateIoInterrupt(process, onProcessUpdated, readyQueue, delayFunc, token);

                        break;
                    }

                    // Executa um tick de CPU.
                    await delayFunc(token);
                }

                // Se o processo não bloqueou para I/O, atualiza o RemainingTime.
                if (process.State != ProcessState.Blocked)
                {
                    process.RemainingTime -= executedTicks;
                    if (process.RemainingTime <= 0)
                    {
                        process.State = ProcessState.Completed;
                        readyQueue.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        process.State = ProcessState.Ready;
                        readyQueue.RemoveAt(i);
                        readyQueue.Add(process);
                        i--;
                    }

                    onProcessUpdated(process);
                }
                else
                {
                    // Se bloqueou, o processo já foi removido da fila e será reinserido após o I/O.
                    readyQueue.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    private static async Task SimulateIoInterrupt(Process process, Action<Process> onProcessUpdated, List<Process> readyQueue, Func<CancellationToken, Task> delayFunc, CancellationToken token)
    {
        // Simula a operação de I/O aguardando IoTime ticks.
        for (int i = 0; i < process.IoTime && !token.IsCancellationRequested; i++)
        {
            await delayFunc(token);
        }

        // Ao concluir o I/O, simula a interrupção: atualiza o estado para Ready e notifica.
        process.State = ProcessState.Ready;
        onProcessUpdated(process);

        // Reinsere o processo na fila dos prontos.
        readyQueue.Add(process);
    }
}
