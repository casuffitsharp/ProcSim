using ProcSim.Core.Enums;

namespace ProcSim.Core.Entities;

public class Process
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ExecutionTime { get; set; }
    public int RemainingTime { get; set; }
    public ProcessState State { get; set; } = ProcessState.Ready;
}
