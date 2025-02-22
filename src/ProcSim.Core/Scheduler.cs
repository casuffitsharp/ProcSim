using ProcSim.Core.Entities;
using ProcSim.Core.Enums;

namespace ProcSim.Core;

public class Scheduler
{
    private readonly Queue<Process> _queue = new();

    public void AddProcess(Process process)
    {
        _queue.Enqueue(process);
    }

    public void Run()
    {
        while (_queue.Any())
        {
            Process process = _queue.Dequeue();
            process.State = ProcessState.Running;
            // Simple FCFS logic: run until complete
            while (process.RemainingTime > 0)
            {
                process.RemainingTime--;
            }
            process.State = ProcessState.Completed;
        }
    }
}
