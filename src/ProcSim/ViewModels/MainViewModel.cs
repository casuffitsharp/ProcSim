using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcSim.New.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;

namespace ProcSim.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly SimulationController _simulator;

    public MainViewModel(SimulationController simulationController, VmConfigViewModel vmConfig, ProcessesConfigViewModel processesConfig/*, TaskManagerViewModel taskManagerVm*/)
    {
        _simulator = simulationController;

        VmConfig = vmConfig;
        ProcessesConfig = processesConfig;
        //TaskManagerVm = taskManagerVm;

        //ReadyProcessesView = CreateView(Processes, p => p.State == ProcessState.Ready);
        //RunningProcessesView = CreateView(Processes, p => p.State == ProcessState.Running);
        //BlockedProcessesView = CreateView(Processes, p => p.State == ProcessState.Blocked);
        //CompletedProcessesView = CreateView(Processes, p => p.State == ProcessState.Completed);

        //RunPauseSchedulingCommand = new AsyncRelayCommand(RunPauseSchedulingAsync, CanRunPauseScheduling, AsyncRelayCommandOptions.AllowConcurrentExecutions);
        //ResetSchedulingCommand = new RelayCommand(ResetScheduling, CanResetScheduling);

        VmConfig.PropertyChanged += CurrentFile_PropertyChanged;
        ProcessesConfig.PropertyChanged += CurrentFile_PropertyChanged;

        //_simulator.TickManager.RunStateChanged += UpdateRunState;
        //_simulator.SimulationStateChanged += UpdateRunState;
    }

    public VmConfigViewModel VmConfig { get; }
    public ProcessesConfigViewModel ProcessesConfig { get; }
    //public TaskManagerViewModel TaskManagerVm { get; }

    public ObservableCollection<ProcessConfigViewModel> Processes => ProcessesConfig.Processes;

    public ICollectionView ReadyProcessesView { get; }
    public ICollectionView RunningProcessesView { get; }
    public ICollectionView BlockedProcessesView { get; }
    public ICollectionView CompletedProcessesView { get; }

    public IAsyncRelayCommand RunPauseSchedulingCommand { get; }
    public IRelayCommand ResetSchedulingCommand { get; }

    //public ushort TickInterval
    //{
    //    get => _simulator.TickManager.TickInterval;
    //    set
    //    {
    //        if (TickInterval != value)
    //        {
    //            _simulator.TickManager.TickInterval = value;
    //            OnPropertyChanged();
    //        }
    //    }
    //}

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RunPauseSchedulingCommand))]
    [NotifyCanExecuteChangedFor(nameof(ResetSchedulingCommand))]
    public partial bool IsRunning { get; private set; }

    public string StatusBarMessage => $"VM: {Path.GetFileNameWithoutExtension(VmConfig.CurrentFile ?? "New")} | Proc: {Path.GetFileNameWithoutExtension(ProcessesConfig.CurrentFile ?? "New")}";

    //private bool CanRunPauseScheduling()
    //{
    //    return IsRunning || Processes.Any(p => p.State != ProcessState.Completed);
    //}

    //private async Task RunPauseSchedulingAsync()
    //{
    //    if (_simulator.IsRunning)
    //    {
    //        _simulator.Pause();
    //    }
    //    else
    //    {
    //        if (!_simulator.HasStarted)
    //        {
    //            List<Process> processes = [.. Processes.Select(p => p.Model)];
    //            List<IoDeviceConfig> devices = [.. VmSettingsVm.AvailableDevices.Where(d => d.IsEnabled).Select(d => d.MapToModel())];

    //            _simulator.Initialize(processes, devices, algorithmType: VmSettingsVm.SelectedAlgorithm, quantum: VmSettingsVm.Quantum, cores: VmSettingsVm.CpuCores);
    //            TaskManagerVm.Initialize(_simulator.PerformanceMonitor);
    //        }

    //        await _simulator.StartAsync();
    //    }
    //}

    private bool CanResetScheduling()
    {
        return !IsRunning;
    }

    //private void ResetScheduling()
    //{
    //    _simulator.ResetAsync();

    //    foreach (ProcessViewModel vm in Processes)
    //        vm.Model.Reset();

    //    RunPauseSchedulingCommand.NotifyCanExecuteChanged();
    //}

    //private void UpdateRunState()
    //{
    //    IsRunning = _simulator.IsRunning;
    //    VmSettingsVm.CanChangeConfigs = !_simulator.HasStarted;
    //    ProcessesConfig.CanChangeConfigs = !_simulator.HasStarted;
    //}

    private void CurrentFile_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(VmConfig.CurrentFile) or nameof(ProcessesConfig.CurrentFile))
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
