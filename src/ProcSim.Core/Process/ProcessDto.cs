namespace ProcSim.Core.Process;

public record ProcessDto
{
    public Dictionary<string, int> Registers { get; set; } = [];
    public List<Instruction> Instructions { get; set; } = [];
}