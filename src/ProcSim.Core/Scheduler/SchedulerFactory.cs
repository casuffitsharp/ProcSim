using ProcSim.Core.Process;

namespace ProcSim.Core.Scheduler;

internal static class SchedulerFactory
{
    public static IScheduler Create(SchedulerType schedulerType, Kernel kernel, IReadOnlyDictionary<uint, PCB> idlePcbs)
    {
        return schedulerType switch
        {
            SchedulerType.RoundRobin => new RoundRobinScheduler(idlePcbs),
            SchedulerType.Priority => new PriorityScheduler(idlePcbs, kernel),
            _ => throw new NotImplementedException()
        };
    }
}
