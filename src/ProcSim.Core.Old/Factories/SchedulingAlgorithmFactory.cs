using ProcSim.Core.Old.Enums;
using ProcSim.Core.Old.Scheduling.Algorithms;
using ProcSim.Core.Old.SystemCalls;
using ProcSim.Core.Scheduling.Algorithms;

namespace ProcSim.Core.Old.Factories;

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
