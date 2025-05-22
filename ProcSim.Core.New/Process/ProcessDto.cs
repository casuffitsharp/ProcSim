namespace ProcSim.Core.New.Process;

public record ProcessDto
{
    public Dictionary<string, uint> Registers { get; set; }
    public List<Instruction> Instructions { get; set; }
}