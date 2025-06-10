namespace ProcSim.Core.Process;

public record ProcessDto
{
    public string Name { get; set; }
    public ProcessStaticPriority Priority { get; set; }
    public Dictionary<string, int> Registers { get; set; } = [];
    public List<Instruction> Instructions { get; set; } = [];
}
