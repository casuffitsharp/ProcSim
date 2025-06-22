using CommunityToolkit.Mvvm.ComponentModel;

namespace ProcSim.ViewModels;

public partial class TaskManagerViewModel(TaskManagerDetailsViewModel detailsVm, CpuMonitoringViewModel cpuMonitoringVm, DevicesMonitoringViewModel devicesMonitoringVm, ProcessesHistoryViewModel processesHistoryVm) : ObservableObject
{
    public TaskManagerDetailsViewModel DetailsVm { get; } = detailsVm;
    public CpuMonitoringViewModel CpuMonitoringVm { get; } = cpuMonitoringVm;
    public DevicesMonitoringViewModel DevicesMonitoringVm { get; } = devicesMonitoringVm;
    public ProcessesHistoryViewModel ProcessesHistoryVm { get; } = processesHistoryVm;
}
