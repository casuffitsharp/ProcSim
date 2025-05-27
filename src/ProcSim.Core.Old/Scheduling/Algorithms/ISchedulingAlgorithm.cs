using ProcSim.Core.Old.Runtime;
using System.Collections.Concurrent;

namespace ProcSim.Core.Old.Scheduling.Algorithms;

public interface ISchedulingAlgorithm
{
    ConcurrentQueue<PCB> RunQueue { get; }
    PCB PickNext();
}
