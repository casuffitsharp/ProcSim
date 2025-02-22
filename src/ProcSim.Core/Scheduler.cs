using ProcSim.Core.Entities;
using ProcSim.Core.Enums;

namespace ProcSim.Core;

public class Scheduler
{
    private readonly Queue<Process> _queue = new();

    public event Action<Process> ProcessUpdated;

    public void AddProcess(Process process)
    {
        _queue.Enqueue(process);
    }

    public void Run()
    {
        while (_queue.Count > 0)
        {
            Process process = _queue.Dequeue();
            process.State = ProcessState.Running;

            while (process.RemainingTime > 0)
            {
                Thread.Sleep(2000);
                process.RemainingTime--;
                ProcessUpdated.Invoke(process);
            }

            process.State = ProcessState.Completed;
            ProcessUpdated.Invoke(process);
        }
    }
}
