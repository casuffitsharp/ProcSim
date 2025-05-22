using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Core.New.Process;

namespace ProcSim.New.ViewModels;

public class ProcessStatusViewModel : ObservableObject
{
    public uint ProcessId { get; }
    public int Priority { get; }
    public ProcessState State { get; }
    public string CurrentInstruction { get; }
}
public class CpuStatusViewModel : ObservableObject
{
    public uint CoreId { get; }
    public uint CurrentProcessId { get; }
    public int RemainingQuantum { get; }
}

public class IoChannelStatusViewModel : ObservableObject
{
    public uint DeviceId { get; }
    public uint ChannelId { get; }
    public uint? CurrentProcessId { get; } // nulo se idle
    public int QueueLength { get; }
}
