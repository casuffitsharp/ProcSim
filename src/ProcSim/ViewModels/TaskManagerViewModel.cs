using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Core.Monitoring;
using ProcSim.Core.Process;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Threading;

namespace ProcSim.ViewModels;

public class TaskManagerViewModel : ObservableObject
{
    private readonly Dictionary<int, ProcessDetailsViewModel> _mapByPid;
    private readonly Dispatcher _uiDispatcher;
    private readonly MonitoringService _monitoringService;

    public TaskManagerViewModel(MonitoringService monitoringService)
    {
        _monitoringService = monitoringService;
        _uiDispatcher = Dispatcher.CurrentDispatcher;

        ProcessesDetails = [];
        _mapByPid = [];

        _monitoringService.ProcessListUpdated += OnProcessListUpdated;
        _monitoringService.OnReset += Reset;

        RunningProcessesDetails = CreateView(ProcessesDetails, p => p.State != ProcessState.Terminated);
        TerminatedProcessesDetails = CreateView(ProcessesDetails, p => p.State == ProcessState.Terminated);
    }

    public ObservableCollection<ProcessDetailsViewModel> ProcessesDetails { get; set; }
    public ICollectionView RunningProcessesDetails { get; }
    public ICollectionView TerminatedProcessesDetails { get; }

    public void Reset()
    {
        _uiDispatcher.Invoke(() =>
        {
            ProcessesDetails.Clear();
            _mapByPid.Clear();
        }, DispatcherPriority.Background);
    }

    private void OnProcessListUpdated(IReadOnlyList<ProcessSnapshot> processesSnapshots)
    {
        _uiDispatcher.Invoke(() =>
        {
            HashSet<int> seen = new(processesSnapshots.Count);

            foreach (ProcessSnapshot snapshot in processesSnapshots)
            {
                seen.Add(snapshot.Pid);
                if (_mapByPid.TryGetValue(snapshot.Pid, out ProcessDetailsViewModel existingVm))
                {
                    existingVm.UpdateFromSnapshot(snapshot);
                }
                else
                {
                    ProcessDetailsViewModel viewModel = new(snapshot);
                    ProcessesDetails.Add(viewModel);
                    _mapByPid[snapshot.Pid] = viewModel;
                }
            }
        }, DispatcherPriority.Background);
    }

    private static ICollectionView CreateView<T>(ObservableCollection<T> source, Func<T, bool> predicate)
    {
        ICollectionView view = new CollectionViewSource { Source = source }.View;
        view.Filter = o => o is T t && predicate(t);
        if (view is ICollectionViewLiveShaping live)
        {
            live.IsLiveFiltering = true;
            live.LiveFilteringProperties.Add(nameof(ProcessDetailsViewModel.State));
        }
        return view;
    }
}

public partial class ProcessDetailsViewModel : ObservableObject
{
    public ProcessDetailsViewModel(ProcessSnapshot s)
    {
        Pid = s.Pid;
        Name = s.Name;
        State = s.State;
        CurrentOperation = "";
        Cpu = s.CpuUsage;
        StaticPriority = s.StaticPriority;
        DynamicPriority = s.DynamicPriority;
    }

    [ObservableProperty] public partial string Name { get; set; }
    [ObservableProperty] public partial int Pid { get; set; }
    [ObservableProperty] public partial ProcessState State { get; set; }
    [ObservableProperty] public partial string CurrentOperation { get; set; }
    [ObservableProperty] public partial ushort Cpu { get; set; }
    [ObservableProperty] public partial ProcessStaticPriority StaticPriority { get; set; }
    [ObservableProperty] public partial int DynamicPriority { get; set; }

    public void UpdateFromSnapshot(ProcessSnapshot s)
    {
        State = s.State;
        CurrentOperation = ""; //TODO
        Cpu = s.CpuUsage;
        StaticPriority = s.StaticPriority;
        DynamicPriority = s.DynamicPriority;
    }
}