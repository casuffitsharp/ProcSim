using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ProcSim.Core;
using ProcSim.Core.Enums;
using ProcSim.Core.Models;

namespace ProcSim.Wpf.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly Scheduler _scheduler = new();
    private CancellationTokenSource _cts = new();

    private readonly ObservableCollection<ProcessViewModel> _processes = [];

    private readonly ObservableCollection<ProcessViewModel> _readyProcesses = [];
    private readonly ObservableCollection<ProcessViewModel> _runningProcesses = [];
    private readonly ObservableCollection<ProcessViewModel> _blockedProcesses = [];
    private readonly ObservableCollection<ProcessViewModel> _completedProcesses = [];

    public ReadOnlyObservableCollection<ProcessViewModel> ReadyProcesses { get; }
    public ReadOnlyObservableCollection<ProcessViewModel> RunningProcesses { get; }
    public ReadOnlyObservableCollection<ProcessViewModel> BlockedProcesses { get; }
    public ReadOnlyObservableCollection<ProcessViewModel> CompletedProcesses { get; }

    public ProcessRegistrationViewModel ProcessRegistrationViewModel { get; }
    public SimulationSettingsViewModel SimulationSettingsViewModel { get; }

    public MainViewModel()
    {
        _scheduler.ProcessUpdated += OnProcessUpdated;

        ProcessRegistrationViewModel = new ProcessRegistrationViewModel(_processes);
        SimulationSettingsViewModel = new SimulationSettingsViewModel();

        ReadyProcesses = new ReadOnlyObservableCollection<ProcessViewModel>(_readyProcesses);
        RunningProcesses = new ReadOnlyObservableCollection<ProcessViewModel>(_runningProcesses);
        BlockedProcesses = new ReadOnlyObservableCollection<ProcessViewModel>(_blockedProcesses);
        CompletedProcesses = new ReadOnlyObservableCollection<ProcessViewModel>(_completedProcesses);

        _processes.CollectionChanged += Processes_CollectionChanged;
    }

    public void AddProcess(ProcessViewModel processViewModel)
    {
        _processes.Add(processViewModel);
    }

    public async Task RunSchedulingAsync()
    {
        if (!_processes.Any()) return;

        _cts = new CancellationTokenSource();
        var algorithm = SimulationSettingsViewModel.SelectedAlgorithmInstance;

        await _scheduler.RunAsync(new(_processes.Select(p => p.Model)), algorithm, _cts.Token);
        UpdateFilteredLists();
    }

    public void CancelScheduling()
    {
        _cts.Cancel();
    }

    public void Reset()
    {
        CancelScheduling();
        _processes.Clear();
        UpdateFilteredLists();
    }

    private void OnProcessUpdated(Process updatedProcess)
    {
        var processViewModel = _processes.FirstOrDefault(p => p.Model == updatedProcess);
        if (processViewModel is null)
            return;

        processViewModel.UpdateFromModel();
        UpdateFilteredLists();
    }

    private void Processes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateFilteredLists();
    }

    private void UpdateFilteredLists()
    {
        _readyProcesses.Clear();
        _runningProcesses.Clear();
        _blockedProcesses.Clear();
        _completedProcesses.Clear();

        foreach (var process in _processes)
        {
            switch (process.State)
            {
                case ProcessState.Ready:
                    _readyProcesses.Add(process);
                    break;
                case ProcessState.Running:
                    _runningProcesses.Add(process);
                    break;
                case ProcessState.Blocked:
                    _blockedProcesses.Add(process);
                    break;
                case ProcessState.Completed:
                    _completedProcesses.Add(process);
                    break;
            }
        }

        OnPropertyChanged(nameof(ReadyProcesses));
        OnPropertyChanged(nameof(RunningProcesses));
        OnPropertyChanged(nameof(BlockedProcesses));
        OnPropertyChanged(nameof(CompletedProcesses));
    }
}
