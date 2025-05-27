using ProcSim.Core.Process;
using System.Collections.Concurrent;

namespace ProcSim.Core.Scheduler;

internal static class SchedulerFactory
{
    public static IScheduler Create(SchedulerType schedulerType, IReadOnlyDictionary<uint, PCB> idlePcbs)
    {
        return schedulerType switch
        {
            SchedulerType.RoundRobin => new RoundRobinScheduler(idlePcbs),
            SchedulerType.Priority => new PriorityScheduler(idlePcbs),
            _ => throw new NotImplementedException()
        };
    }
}
