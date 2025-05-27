using ProcSim.Core.Old.Enums;

namespace ProcSim.Core.Old.Runtime;

public class PCB
{
    public ProcessState State { get; set; }
    public uint Counter { get; set; }
    public IRegister Register { get; set; }
    public uint Priority { get; set; }
    public uint CpuTime { get; set; }
}
