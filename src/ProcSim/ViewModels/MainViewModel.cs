using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Core.Monitoring;
using ProcSim.New.ViewModels;
using System.ComponentModel;
using System.IO;

namespace ProcSim.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public MainViewModel(MonitoringService monitoringService, SimulationControlViewModel simulationControl, VmConfigViewModel vmConfig, ProcessesConfigViewModel processesConfig, TaskManagerViewModel taskManagerVm)
    {
        MonitoringService = monitoringService;
        SimulationControl = simulationControl;

        VmConfig = vmConfig;
        ProcessesConfig = processesConfig;
        TaskManagerVm = taskManagerVm;

        VmConfig.CurrentFileChanged += CurrentFile_PropertyChanged;
        ProcessesConfig.CurrentFileChanged += CurrentFile_PropertyChanged;
    }

    public VmConfigViewModel VmConfig { get; }
    public ProcessesConfigViewModel ProcessesConfig { get; }
    public MonitoringService MonitoringService { get; }
    public SimulationControlViewModel SimulationControl { get; }
    public TaskManagerViewModel TaskManagerVm { get; }

    public string StatusBarMessage => $"VM: {Path.GetFileNameWithoutExtension(VmConfig.CurrentFile ?? "New")} | Proc: {Path.GetFileNameWithoutExtension(ProcessesConfig.CurrentFile ?? "New")}";

    private void CurrentFile_PropertyChanged()
    {
        OnPropertyChanged(nameof(StatusBarMessage));
    }
}
