namespace ProcSim.Core.Scheduling;

public interface IPreemptiveAlgorithm
{
    public int Quantum { get; set; }
}
