using System.Collections.Concurrent;

namespace ProcSim.Core.New.Process;

public class PCB
{
    public PCB(uint processId, Dictionary<string, uint> registers, uint priority)
    {
        for (uint i = 0; i < 8; i++)
        {
            uint value = registers?.TryGetValue($"R{i}", out uint regValue) == true ? regValue : 0;
            Registers[$"R{i}"] = value;
        }

        Registers["ZF"] = 0;
        Registers["CF"] = 0;
        ProcessId = processId;
        Priority = priority;
    }

    public ProcessState State { get; set; } = ProcessState.New;
    public uint ProgramCounter { get; set; }
    public uint StackPointer { get; set; }
    public uint Priority { get; set; }
    public ConcurrentDictionary<string, uint> Registers { get; } = new();
    public uint ProcessId { get; }
    public ulong CpuCycles { get; private set; }
    public ulong WaitCycles { get; private set; }
    internal ulong LastDispatchCycle { get; set; }
    internal ulong LastWaitCycle { get; set; }

    internal void OnDispatched(ulong currentCycle)
    {
        LastDispatchCycle = currentCycle;
    }
    internal void OnPreempted(ulong currentCycle)
    {
        CpuCycles += currentCycle - LastDispatchCycle;
    }

    // chamadas no IoInterruptHandler, por exemplo:
    internal void OnIoRequested(ulong currentCycle)
    {
        LastWaitCycle = currentCycle;
    }
    internal void OnIoCompleted(ulong currentCycle)
    {
        WaitCycles += currentCycle - LastWaitCycle;
    }
}
