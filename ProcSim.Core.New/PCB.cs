using System.Collections.Concurrent;

namespace ProcSim.Core.New;

public record PCB
{
    public ProcessState State { get; set; } = ProcessState.New;
    public uint ProgramCounter { get; set; }
    public uint StackPointer { get; set; }
    public ConcurrentDictionary<string, uint> Registers { get; } = new();
    public uint ProcessId { get; }

    public PCB(uint processId)
    {
        for (uint i = 0; i < 8; i++)
            Registers[$"R{i}"] = 0;

        Registers["ZF"] = 0;
        Registers["CF"] = 0;
        Registers["SP"] = 0;
        ProcessId = processId;
    }
}

public enum ProcessState
{
    New,
    Ready,
    Running,
    Waiting,
    Terminated
}

public enum SyscallType
{
    Read,
    Write,
    Exit,
    Fork,
    Exec,
    IoRequest
}

public record MicroOp(string Name, Action<CPU> Execute);

public class Instruction(string Mnemonic, IEnumerable<MicroOp> microOps)
{
    public Queue<MicroOp> MicroOps { get; } = new(microOps);
    public string Mnemonic { get; } = Mnemonic;
}
