using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcSim.Core.Monitoring;
using ProcSim.Core.Process;
using ProcSim.Core.Simulation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Threading;

namespace ProcSim.ViewModels;

public partial class TaskManagerDetailsViewModel : ObservableObject
{
    private readonly Dictionary<int, TaskManagerProcessDetailsViewModel> _mapByPid;
    private readonly Dispatcher _uiDispatcher;
    private readonly MonitoringService _monitoringService;
    private readonly SimulationController _simulationController;

    public TaskManagerDetailsViewModel(MonitoringService monitoringService, SimulationController simulationController)
    {
        _monitoringService = monitoringService;
        _simulationController = simulationController;
        _uiDispatcher = Dispatcher.CurrentDispatcher;

        ProcessesDetails = [];
        _mapByPid = [];

        _monitoringService.ProcessListUpdated += OnProcessListUpdated;
        _monitoringService.OnReset += () => _uiDispatcher.Invoke(() => Reset, DispatcherPriority.Background);

        RunningProcessesDetails = CreateView(ProcessesDetails, p => p.State != ProcessState.Terminated);
        TerminatedProcessesDetails = CreateView(ProcessesDetails, p => p.State == ProcessState.Terminated);

        TerminateProcessCommand = new RelayCommand<TaskManagerProcessDetailsViewModel>(TerminateProcess, CanTerminateProcess);
    }

    public static ProcessStaticPriority[] ProcessStaticPriorityValues { get; } = [.. Enum.GetValues<ProcessStaticPriority>()];

    [ObservableProperty]
    public partial TaskManagerProcessDetailsViewModel SelectedProcess { get; set; }

    public ObservableCollection<TaskManagerProcessDetailsViewModel> ProcessesDetails { get; set; }
    public ICollectionView RunningProcessesDetails { get; }
    public ICollectionView TerminatedProcessesDetails { get; }

    public IRelayCommand<TaskManagerProcessDetailsViewModel> TerminateProcessCommand { get; }

    public void Reset()
    {
        ProcessesDetails.Clear();
        _mapByPid.Clear();
    }

    private void OnProcessListUpdated(IReadOnlyList<ProcessSnapshot> processesSnapshots)
    {
        _uiDispatcher.Invoke(() =>
        {
            HashSet<int> seen = new(processesSnapshots.Count);

            foreach (ProcessSnapshot snapshot in processesSnapshots)
            {
                seen.Add(snapshot.Pid);
                if (_mapByPid.TryGetValue(snapshot.Pid, out TaskManagerProcessDetailsViewModel existingVm))
                {
                    existingVm.UpdateFromSnapshot(snapshot);
                }
                else
                {
                    bool isUserProcess = _simulationController.IsUserProcess(snapshot.Pid);
                    TaskManagerProcessDetailsViewModel viewModel = new(snapshot, _simulationController, isUserProcess);
                    ProcessesDetails.Add(viewModel);
                    _mapByPid[snapshot.Pid] = viewModel;
                }
            }
        }, DispatcherPriority.Background);
    }

    private void TerminateProcess(TaskManagerProcessDetailsViewModel processToTerminate)
    {
        if (processToTerminate == null)
            return;

        _simulationController.TerminateProcess(processToTerminate.Pid);
    }

    private static bool CanTerminateProcess(TaskManagerProcessDetailsViewModel process)
    {
        return process?.IsUserProcess == true && process.State != ProcessState.Terminated;
    }

    private static ICollectionView CreateView<T>(ObservableCollection<T> source, Func<T, bool> predicate)
    {
        ICollectionView view = new CollectionViewSource { Source = source }.View;
        view.Filter = o => o is T t && predicate(t);
        if (view is ICollectionViewLiveShaping live)
        {
            live.IsLiveFiltering = true;
            live.LiveFilteringProperties.Add(nameof(TaskManagerProcessDetailsViewModel.State));
        }

        return view;
    }
}
