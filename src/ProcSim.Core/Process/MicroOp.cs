namespace ProcSim.Core.Process;

public record MicroOp(string Name, string Description, Action<CPU> Execute);
