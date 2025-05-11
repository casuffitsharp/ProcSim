using ProcSim.Core.Runtime;
using System.Collections.Concurrent;

namespace ProcSim.Core.Scheduling.Algorithms;

public interface ISchedulingAlgorithm
{
    ConcurrentQueue<PCB> RunQueue { get; }
    PCB PickNext();
}
