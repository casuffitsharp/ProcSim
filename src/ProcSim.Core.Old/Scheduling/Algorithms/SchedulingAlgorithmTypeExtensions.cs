using ProcSim.Core.Old.Enums;

namespace ProcSim.Core.Old.Scheduling.Algorithms;

public static class SchedulingAlgorithmTypeExtensions
{
    public static bool IsPreemptive(this SchedulingAlgorithmType algorithmType)
    {
        return algorithmType switch
        {
            SchedulingAlgorithmType.None or SchedulingAlgorithmType.Fcfs => false,
            SchedulingAlgorithmType.RoundRobin => true,
            _ => throw new NotImplementedException(),
        };
    }
}
