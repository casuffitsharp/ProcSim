namespace ProcSim.Core.Scheduling.Algorithms;

public interface IPreemptiveAlgorithm
{
    uint Quantum { get; set; }
}
