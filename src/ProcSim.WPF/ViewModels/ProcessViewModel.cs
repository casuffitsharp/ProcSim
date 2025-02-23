using ProcSim.Core.Enums;
using ProcSim.Core.Models;

namespace ProcSim.WPF.ViewModels;

public class ProcessViewModel(Process process) : ViewModelBase
{
    public Process Model => process;

    public int Id => process.Id;
    public string Name => process.Name;
    public int ExecutionTime => process.ExecutionTime;
    public int IoTime => process.IoTime;
    public int RemainingTime => process.RemainingTime;
    public ProcessState State => process.State;
}
