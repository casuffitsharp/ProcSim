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

        //ReadyProcessesView = CreateView(Processes, p => p.State == ProcessState.Ready);
        //RunningProcessesView = CreateView(Processes, p => p.State == ProcessState.Running);
        //BlockedProcessesView = CreateView(Processes, p => p.State == ProcessState.Blocked);
        //CompletedProcessesView = CreateView(Processes, p => p.State == ProcessState.Completed);

        VmConfig.CurrentFileChanged += CurrentFile_PropertyChanged;
        ProcessesConfig.CurrentFileChanged += CurrentFile_PropertyChanged;
    }

    public VmConfigViewModel VmConfig { get; }
    public ProcessesConfigViewModel ProcessesConfig { get; }
    public MonitoringService MonitoringService { get; }
    public SimulationControlViewModel SimulationControl { get; }
    public TaskManagerViewModel TaskManagerVm { get; }

    //public ObservableCollection<ProcessConfigViewModel> Processes => ProcessesConfig.Processes;

    public ICollectionView ReadyProcessesView { get; }
    public ICollectionView RunningProcessesView { get; }
    public ICollectionView BlockedProcessesView { get; }
    public ICollectionView CompletedProcessesView { get; }

    public string StatusBarMessage => $"VM: {Path.GetFileNameWithoutExtension(VmConfig.CurrentFile ?? "New")} | Proc: {Path.GetFileNameWithoutExtension(ProcessesConfig.CurrentFile ?? "New")}";

    private void CurrentFile_PropertyChanged()
    {
        OnPropertyChanged(nameof(StatusBarMessage));
    }

    //private static ICollectionView CreateView<T>(ObservableCollection<T> source, Func<T, bool> predicate)
    //{
    //    ICollectionView view = new CollectionViewSource { Source = source }.View;
    //    view.Filter = o => o is T t && predicate(t);
    //    if (view is ICollectionViewLiveShaping live)
    //    {
    //        live.IsLiveFiltering = true;
    //        live.LiveFilteringProperties.Add(nameof(ProcessViewModel.State));
    //    }
    //    return view;
    //}
}
