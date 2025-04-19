using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcSim.Core.Enums;
using ProcSim.Core.IO.Devices;
using ProcSim.Core.Models;
using ProcSim.Core.Simulation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;

namespace ProcSim.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ISimulationController _simulator;

    public MainViewModel(ISimulationController simulationController, VmSettingsViewModel vmSettingsVm, ProcessesSettingsViewModel processesSettingsVm, TaskManagerViewModel taskManagerVm)
    {
        _simulator = simulationController;

        VmSettingsVm = vmSettingsVm;
        ProcessesSettingsVm = processesSettingsVm;
        TaskManagerVm = taskManagerVm;

        ReadyProcessesView = CreateView(Processes, p => p.State == ProcessState.Ready);
        RunningProcessesView = CreateView(Processes, p => p.State == ProcessState.Running);
        BlockedProcessesView = CreateView(Processes, p => p.State == ProcessState.Blocked);
        CompletedProcessesView = CreateView(Processes, p => p.State == ProcessState.Completed);

        RunPauseSchedulingCommand = new AsyncRelayCommand(RunPauseSchedulingAsync, CanRunPauseScheduling, AsyncRelayCommandOptions.AllowConcurrentExecutions);
        ResetSchedulingCommand = new RelayCommand(ResetScheduling, CanResetScheduling);

        VmSettingsVm.PropertyChanged += CurrentFile_PropertyChanged;
        ProcessesSettingsVm.PropertyChanged += CurrentFile_PropertyChanged;

        _simulator.TickManager.RunStateChanged += UpdateRunState;
        _simulator.SimulationStateChanged += UpdateRunState;
    }

    public VmSettingsViewModel VmSettingsVm { get; }
    public ProcessesSettingsViewModel ProcessesSettingsVm { get; }
    public TaskManagerViewModel TaskManagerVm { get; }

    public ObservableCollection<ProcessViewModel> Processes => ProcessesSettingsVm.Processes;

    public ICollectionView ReadyProcessesView { get; }
    public ICollectionView RunningProcessesView { get; }
    public ICollectionView BlockedProcessesView { get; }
    public ICollectionView CompletedProcessesView { get; }

    public IAsyncRelayCommand RunPauseSchedulingCommand { get; }
    public IRelayCommand ResetSchedulingCommand { get; }

    public ushort TickInterval
    {
        get => _simulator.TickManager.TickInterval;
        set
        {
            if (TickInterval != value)
            {
                _simulator.TickManager.TickInterval = value;
                OnPropertyChanged();
            }
        }
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RunPauseSchedulingCommand))]
    [NotifyCanExecuteChangedFor(nameof(ResetSchedulingCommand))]
    public partial bool IsRunning { get; private set; }

    public string StatusBarMessage => $"VM: {Path.GetFileNameWithoutExtension(VmSettingsVm.CurrentFile ?? "New")} | Proc: {Path.GetFileNameWithoutExtension(ProcessesSettingsVm.CurrentFile ?? "New")}";

    private bool CanRunPauseScheduling()
    {
        return IsRunning || Processes.Any(p => p.State != ProcessState.Completed);
    }

    private async Task RunPauseSchedulingAsync()
    {
        if (_simulator.IsRunning)
        {
            _simulator.Pause();
        }
        else
        {
            if (!_simulator.HasStarted)
            {
                List<Process> processes = [.. Processes.Select(p => p.Model)];
                List<IoDeviceConfig> devices = [.. VmSettingsVm.AvailableDevices.Where(d => d.IsEnabled).Select(d => d.MapToDeviceConfig())];

                _simulator.Initialize(processes, devices, algorithmType: VmSettingsVm.SelectedAlgorithm, quantum: VmSettingsVm.Quantum, cores: VmSettingsVm.CpuCores);
                TaskManagerVm.Initialize(_simulator.PerformanceMonitor);
            }

            await _simulator.StartAsync();
        }
    }

    private bool CanResetScheduling()
    {
        return !IsRunning;
    }

    private void ResetScheduling()
    {
        _simulator.ResetAsync();

        foreach (ProcessViewModel vm in Processes)
            vm.Model.Reset();

        RunPauseSchedulingCommand.NotifyCanExecuteChanged();
    }

    private void UpdateRunState()
    {
        IsRunning = _simulator.IsRunning;
        VmSettingsVm.CanChangeConfigs = !_simulator.HasStarted;
        ProcessesSettingsVm.CanChangeConfigs = !_simulator.HasStarted;
    }

    private void CurrentFile_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(VmSettingsVm.CurrentFile) or nameof(ProcessesSettingsVm.CurrentFile))
            OnPropertyChanged(nameof(StatusBarMessage));
    }

    private static ICollectionView CreateView<T>(ObservableCollection<T> source, Func<T, bool> predicate)
    {
        ICollectionView view = new CollectionViewSource { Source = source }.View;
        view.Filter = o => o is T t && predicate(t);
        if (view is ICollectionViewLiveShaping live)
        {
            live.IsLiveFiltering = true;
            live.LiveFilteringProperties.Add(nameof(ProcessViewModel.State));
        }
        return view;
    }
}
