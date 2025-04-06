using ProcSim.Core.Enums;
using ProcSim.Core.Scheduling.Algorithms;
using ProcSim.Core.SystemCalls;

namespace ProcSim.Core.Factories;

public static class SchedulingAlgorithmFactory
{
    public static ISchedulingAlgorithm Create(ISysCallHandler sysCallHandler, SchedulingAlgorithmType type)
    {
        return type switch
        {
            SchedulingAlgorithmType.Fcfs => new FcfsScheduling(sysCallHandler),
            SchedulingAlgorithmType.RoundRobin => new RoundRobinScheduling(sysCallHandler),
            _ => throw new NotImplementedException($"Algoritmo {type} não implementado")
        };
    }
}
