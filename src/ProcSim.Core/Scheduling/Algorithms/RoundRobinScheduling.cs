using ProcSim.Core.Models;

namespace ProcSim.Core.Scheduling.Algorithms;

public sealed class RoundRobinScheduling : PreemptiveAlgorithmBase, ISchedulingAlgorithm
{
    public async Task RunAsync(Queue<Process> processes, Action<Process> onProcessUpdated, Func<CancellationToken, Task> delayFunc, CancellationToken token)
    {
        // Simula um quantum: aguarda ticks equivalentes ao quantum (representando a fatia de tempo do processo).
        int remainingQuantum = Quantum;
        while (processes.Count > 0 && remainingQuantum-- > 0 && !token.IsCancellationRequested)
        {
            // Notifica o processo atual (simplesmente para efeitos de log/atualização).
            onProcessUpdated(processes.Peek());
            await delayFunc(token);
        }

        // Ao final do quantum, rotaciona a fila: retira o primeiro processo e o coloca no final.
        if (processes.Count > 0)
        {
            Process proc = processes.Dequeue();
            processes.Enqueue(proc);
        }
    }
}
