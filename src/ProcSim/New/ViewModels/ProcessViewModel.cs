using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace ProcSim.New.ViewModels;

public enum ExecutionMode { Fixed, Random, Infinite }

public class InstructionViewModel : ObservableObject
{
    public bool IsCpu { get; set; }
    public InstructionType CpuOp { get; set; } // Add/Sub/Mul/Div/Random
    public DurationMode IoMode { get; set; } // Fixed/Random
    public int IoFixed { get; set; }
    public int IoMin { get; set; }
    public int IoMax { get; set; }
    public DeviceSettingViewModel SelectedDevice { get; set; }
}

public class ProcessViewModel : ObservableObject
{
    public string Id { get; set; }  // e.g. “P1” ou “R2”
    public string Name { get; set; }
    public int Priority { get; set; }
    public ExecutionMode ExecMode { get; set; }
    public int FixedCount { get; set; }
    public int MinCount { get; set; }
    public int MaxCount { get; set; }

    public ObservableCollection<InstructionViewModel> Instructions { get; } = [];
}