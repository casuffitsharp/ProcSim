using System.Collections.Concurrent;

namespace ProcSim.Core.Process;

public class PCB
{
    public PCB(int processId, string name, Dictionary<string, int> registers, ProcessStaticPriority staticPriority)
    {
        for (uint i = 0; i < 8; i++)
        {
            int value = registers?.TryGetValue($"R{i}", out int regValue) == true ? regValue : 0;
            Registers[$"R{i}"] = value;
        }

        Registers["ZF"] = 0;
        Registers["CF"] = 0;
        ProcessId = processId;
        StaticPriority = staticPriority;
        Name = name;
        State = ProcessState.New;
    }

    internal ulong LastDispatchCycle { get; set; }
    internal ulong LastWaitCycle { get; set; }
    internal ulong LastEnqueueCycle { get; set; }

    public string Name { get; set; }
    public ProcessStaticPriority StaticPriority { get; set; }
    public uint ProgramCounter { get; set; }
    public uint StackPointer { get; set; }
    public int DynamicPriority { get; set; }
    public ConcurrentDictionary<string, int> Registers { get; } = new();
    public int ProcessId { get; }
    public ulong CpuCycles { get; private set; }
    public ProcessState State { get; set; }

    public ulong UserCycles { get; internal set; }
    public ulong SyscallCycles { get; internal set; }
    public ulong WaitCycles { get; private set; }

    internal void OnDispatched(ulong currentCycle, ulong cpuUserCycles, ulong cpuSyscallCycles)
    {
        LastDispatchCycle = currentCycle;
    }

    internal void OnExitRunning(ulong cpuUserCycles, ulong cpuSyscallCycles)
    {
        LastDispatchCycle = 0;
    }

    internal void OnIoCompleted(ulong currentCycle)
    {
        WaitCycles += currentCycle - LastWaitCycle;
    }
}
