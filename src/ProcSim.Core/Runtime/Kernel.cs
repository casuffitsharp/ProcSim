using ProcSim.Core.Enums;
using ProcSim.Core.Models;
using ProcSim.Core.Scheduling;
using ProcSim.Core.Scheduling.Algorithms;

namespace ProcSim.Core.Runtime;

public sealed class Kernel : IKernel
{
    private readonly List<Process> _processTable = [];
    private readonly CpuScheduler _cpuScheduler;
    private readonly ISchedulingAlgorithm _schedulingAlgorithm;

    public Kernel(TickManager tickManager, CpuScheduler cpuScheduler, ISchedulingAlgorithm schedulingAlgorithm, int cores)
    {
        TickManager = tickManager;
        _cpuScheduler = cpuScheduler;
        _schedulingAlgorithm = schedulingAlgorithm;
        Cores = cores;

        _schedulingAlgorithm.OnProcessTick += (coreId, pid) => OnCoreAccounting?.Invoke(coreId, pid);
    }

    public event Action<int, int?> OnCoreAccounting;
    public TickManager TickManager { get; }
    public int Cores { get; }

    public void RegisterProcess(Process process)
    {
        if (!_processTable.Contains(process))
            _processTable.Add(process);
    }

    public void UnRegisterProcess(Process process)
    {
        _processTable.Remove(process);
    }

    public void ClearProcesses()
    {
        _processTable.Clear();
    }

    public async Task RunAsync(Func<CancellationToken> tokenProvider)
    {
        try
        {
            _cpuScheduler.ClearQueue();
            foreach (Process proc in _processTable)
                _cpuScheduler.EnqueueProcess(proc);

            TickManager.Resume();

            List<Task> tasks = [];
            for (int i = 0; i < Cores; i++)
            {
                int coreId = i;
                tasks.Add(Task.Run(async () =>
                {
                    while (!tokenProvider().IsCancellationRequested && _processTable.Any(p => p.State != ProcessState.Completed))
                    {
                        await _schedulingAlgorithm.RunAsync(_cpuScheduler, coreId, TickManager.DelayFunc, tokenProvider);
                        await TickManager.DelayFunc(tokenProvider());
                    }
                }, tokenProvider()));
            }

            await Task.WhenAll(tasks);
        }
        finally
        {
            TickManager.Pause();
            _cpuScheduler.ClearQueue();
        }
    }
}
