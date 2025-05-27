namespace ProcSim.Core.Process;

public class Instruction(string Mnemonic, IEnumerable<MicroOp> microOps)
{
    public Queue<MicroOp> MicroOps { get; } = new(microOps);
    public string Mnemonic { get; } = Mnemonic;
}
