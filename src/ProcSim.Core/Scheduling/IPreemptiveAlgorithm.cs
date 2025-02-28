namespace ProcSim.Core.Scheduling;

public interface IPreemptiveAlgorithm
{
    int Quantum { get; set; }
}
