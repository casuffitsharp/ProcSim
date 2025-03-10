using ProcSim.Core.Enums;
using ProcSim.Core.Scheduling.Algorithms;

namespace ProcSim.Core.Factories;

public static class SchedulingAlgorithmFactory
{
    public static ISchedulingAlgorithm Create(SchedulingAlgorithmType type)
    {
        return type switch
        {
            SchedulingAlgorithmType.Fcfs => new FcfsScheduling(),
            SchedulingAlgorithmType.RoundRobin => new RoundRobinScheduling(),
            _ => throw new NotImplementedException($"Algoritmo {type} não implementado")
        };
    }
}
