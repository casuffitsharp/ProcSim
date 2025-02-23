using ProcSim.Core.Enums;
using ProcSim.Core.Models;

namespace ProcSim.Core.Scheduling;

public class RoundRobinScheduling(int quantum) : ISchedulingAlgorithm
{
    public bool IsPreemptive => true;

    public async Task RunAsync(Queue<Process> processes, Action<Process> onProcessUpdated, CancellationToken token)
    {
        List<Process> readyQueue = [.. processes];

        while (readyQueue.Count != 0 && !token.IsCancellationRequested)
        {
            for (int i = 0; i < readyQueue.Count; i++)
            {
                Process process = readyQueue[i];
                process.State = ProcessState.Running;
                onProcessUpdated(process);

                int executionTime = Math.Min(quantum, process.RemainingTime);
                await Task.Delay(executionTime * 1000, token);
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
