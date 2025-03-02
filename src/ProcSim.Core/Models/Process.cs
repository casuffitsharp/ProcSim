using ProcSim.Core.Enums;

namespace ProcSim.Core.Models;

public class Process
{
    public Process(int id, string name, int executionTime, int ioTime, ProcessType type)
    {
        Id = id;
        Name = name;
        ExecutionTime = executionTime;
        RemainingTime = executionTime;
        IoTime = ioTime;
        Type = type;
    }

    public int Id { get; }
    public string Name { get; }
    public int ExecutionTime { get; }
    public int RemainingTime { get; set; }
    public int IoTime { get; }
    public ProcessState State { get; set; } = ProcessState.Ready;
    public ProcessType Type { get; }
    public bool IoPerformed{ get; set; }
}
