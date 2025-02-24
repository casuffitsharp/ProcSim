using ProcSim.Core.Enums;
using ProcSim.Core.Models;

namespace ProcSim.Core.Scheduling;

public class FcfsScheduling : ISchedulingAlgorithm
{
    public async Task RunAsync(Queue<Process> processes, Action<Process> onProcessUpdated, CancellationToken token)
    {
        while (processes.Count > 0)
        {
            Process process = processes.Dequeue();
            process.State = ProcessState.Running;
            onProcessUpdated(process);

            while (process.RemainingTime > 0 && !token.IsCancellationRequested)
            {
                await Task.Delay(1000, token);
                process.RemainingTime--;
                onProcessUpdated(process);
            }

            process.State = ProcessState.Completed;
            onProcessUpdated(process);
        }
    }
}
