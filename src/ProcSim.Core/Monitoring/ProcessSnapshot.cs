using ProcSim.Core.Process;

namespace ProcSim.Core.Monitoring;

public record ProcessSnapshot(int Pid, string Name, ProcessState State, ushort CpuUsage, ProcessStaticPriority StaticPriority, int DynamicPriority);