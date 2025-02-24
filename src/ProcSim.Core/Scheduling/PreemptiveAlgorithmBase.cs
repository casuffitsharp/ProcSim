namespace ProcSim.Core.Scheduling;

public class PreemptiveAlgorithmBase : IPreemptiveAlgorithm
{
    public int Quantum
    {
        get;
        set
        {
            field = value > 0 ? value : throw new ArgumentException("Quantum must be greater than 0.");
        }
    } = 1;
}
