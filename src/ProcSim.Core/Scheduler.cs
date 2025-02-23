using ProcSim.Core.Enums;
using ProcSim.Core.Models;

namespace ProcSim.Core;

public class Scheduler
{
    private readonly List<Process> _processes = [];
    private CancellationTokenSource _cts;

    public event Action<Process> ProcessUpdated;

    public IReadOnlyList<Process> Processes => _processes.AsReadOnly();

    public void AddProcess(Process process)
    {
        _processes.Add(process);
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        while (_processes.Any(p => p.State != ProcessState.Completed))
        {
            var process = _processes.FirstOrDefault(p => p.State == ProcessState.Ready);
            if (process is null) break;

            process.State = ProcessState.Running;
            ProcessUpdated?.Invoke(process);

            while (process.RemainingTime > 0)
            {
                if (_cts.Token.IsCancellationRequested)
                    return;

                await Task.Delay(1000, _cts.Token);
                process.RemainingTime--;
                ProcessUpdated?.Invoke(process);
            }

            process.State = ProcessState.Completed;
            ProcessUpdated?.Invoke(process);
        }
    }

    public void Reset()
    {
        _cts?.Cancel();
        _processes.Clear();
    }
}
