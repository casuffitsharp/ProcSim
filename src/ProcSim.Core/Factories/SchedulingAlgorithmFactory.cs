using ProcSim.Core.Enums;
using ProcSim.Core.Scheduling;

namespace ProcSim.Core.Factories;

public static class SchedulingAlgorithmFactory
{
    public static ISchedulingAlgorithm Create(SchedulingAlgorithmType type, int quantum = 1)
    {
        return type switch
        {
            SchedulingAlgorithmType.Fcfs => new FcfsScheduling(),
            SchedulingAlgorithmType.RoundRobin => new RoundRobinScheduling(quantum),
            _ => throw new NotImplementedException($"Algoritmo {type} não implementado")
        };
    }
}
