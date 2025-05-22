namespace ProcSim.Core.New.Process;

public record MicroOp(string Name, Action<CPU> Execute);
