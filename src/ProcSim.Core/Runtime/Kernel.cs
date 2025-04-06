using ProcSim.Core.Enums;
using ProcSim.Core.Models;
using ProcSim.Core.Scheduling;
using ProcSim.Core.Scheduling.Algorithms;

namespace ProcSim.Core.Runtime;

public sealed class Kernel(TickManager tickManager, CpuScheduler cpuScheduler, ISchedulingAlgorithm schedulingAlgorithm, int cores) : IKernel
{
    private readonly CancellationTokenSource _cts = new();
    private readonly List<Process> _processTable = [];

    public ISchedulingAlgorithm SchedulingAlgorithm { get; } = schedulingAlgorithm;
    public int Cores { get; } = cores;

    public void RegisterProcess(Process process)
    {
        if (!_processTable.Contains(process))
            _processTable.Add(process);
    }

    public void UnRegisterProcess(Process process)
    {
        _processTable.Remove(process);
    }

    public async Task RunAsync(Func<CancellationToken> tokenProvider)
    {
        try
        {
            cpuScheduler.ClearQueue();
            foreach (Process process in _processTable)
                cpuScheduler.EnqueueProcess(process);

            tickManager.Resume();

            List<Task> coreTasks = [];
            for (int core = 0; core < Cores; core++)
            {
                int currentCore = core + 1;
                coreTasks.Add(Task.Run(async () =>
                {
                    while (!tokenProvider().IsCancellationRequested && _processTable.Any(p => p.State != ProcessState.Completed))
                    {
                        await SchedulingAlgorithm.RunAsync(cpuScheduler, currentCore, tickManager.DelayFunc, tokenProvider);
                        await tickManager.DelayFunc(tokenProvider());
                    }
                }, tokenProvider()));
            }

            await Task.WhenAll(coreTasks);
        }
        finally
        {
            tickManager.Pause();
            cpuScheduler.ClearQueue();
        }
    }
}
