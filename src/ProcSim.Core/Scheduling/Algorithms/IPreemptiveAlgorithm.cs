namespace ProcSim.Core.Scheduling.Algorithms;

public interface IPreemptiveAlgorithm
{
    int Quantum { get; set; }
}
