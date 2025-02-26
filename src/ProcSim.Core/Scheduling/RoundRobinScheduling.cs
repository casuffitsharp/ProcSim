using ProcSim.Core.Enums;
using ProcSim.Core.Models;

namespace ProcSim.Core.Scheduling;

public class RoundRobinScheduling : PreemptiveAlgorithmBase, ISchedulingAlgorithm
{
    public async Task RunAsync(Queue<Process> processes, Action<Process> onProcessUpdated, Func<CancellationToken, Task> delayFunc, CancellationToken token)
    {
        List<Process> readyQueue = new(processes);

        while (readyQueue.Count != 0 && !token.IsCancellationRequested)
        {
            for (int i = 0; i < readyQueue.Count; i++)
            {
                Process process = readyQueue[i];
                process.State = ProcessState.Running;
                onProcessUpdated(process);

                int executionTime = Math.Min(Quantum, process.RemainingTime);

                for (int j = 0; j < executionTime && !token.IsCancellationRequested; j++)
                    await delayFunc(token);

                process.RemainingTime -= executionTime;

                if (process.RemainingTime <= 0)
                {
                    process.State = ProcessState.Completed;
                    readyQueue.RemoveAt(i);
                    i--;
                }
                else
                {
                    process.State = ProcessState.Ready;
                }

                onProcessUpdated(process);
            }
        }
    }
}
