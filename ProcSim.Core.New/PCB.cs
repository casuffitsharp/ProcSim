using System.Collections.Concurrent;

namespace ProcSim.Core.New;

public class PCB
{
    public ProcessState State { get; set; } = ProcessState.New;
    public uint ProgramCounter { get; set; }
    public uint StackPointer { get; set; }
    public ConcurrentDictionary<string, uint> Registers { get; } = new();
    public uint ProcessId { get; }

    public PCB(uint processId, Dictionary<string, uint> registers)
    {
        for (uint i = 0; i < 8; i++)
        {
            uint value = registers?.TryGetValue($"R{i}", out uint regValue) == true ? regValue : 0;
            Registers[$"R{i}"] = value;
        }

        Registers["ZF"] = 0;
        Registers["CF"] = 0;
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

public record Process
{
    public Dictionary<string, uint> Registers { get; set; }
    public List<Instruction> Instructions { get; set; }
}