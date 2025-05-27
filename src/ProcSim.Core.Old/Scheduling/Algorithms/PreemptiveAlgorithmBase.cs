namespace ProcSim.Core.Old.Scheduling.Algorithms;

public class PreemptiveAlgorithmBase : IPreemptiveAlgorithm
{
    public uint Quantum
    {
        get;
        set
        {
            if (value == 0)
                throw new ArgumentException("Quantum must be greater than 0.");

            field = value;
        }
    } = 1;
}
