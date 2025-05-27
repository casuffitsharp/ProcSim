namespace ProcSim.Core.Process;

public record MicroOp(string Name, Action<CPU> Execute);
