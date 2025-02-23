using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ProcSim.Core;
using ProcSim.Core.Enums;
using ProcSim.Core.Models;

namespace ProcSim.WPF.ViewModels;

public class SchedulerViewModel : ViewModelBase
{
    private readonly Scheduler _scheduler = new();

    private readonly ObservableCollection<ProcessViewModel> _processes = [];

    private readonly ObservableCollection<ProcessViewModel> _readyProcesses = [];
    private readonly ObservableCollection<ProcessViewModel> _blockedProcesses = [];
    private readonly ObservableCollection<ProcessViewModel> _completedProcesses = [];

    public ReadOnlyObservableCollection<ProcessViewModel> ReadyProcesses { get; }
    public ReadOnlyObservableCollection<ProcessViewModel> BlockedProcesses { get; }
    public ReadOnlyObservableCollection<ProcessViewModel> CompletedProcesses { get; }
    public ProcessRegistrationViewModel ProcessRegistrationViewModel { get; }

    public SchedulerViewModel()
    {
        _scheduler.ProcessUpdated += OnProcessUpdated;

        ProcessRegistrationViewModel = new ProcessRegistrationViewModel(_processes);

        // Carrega os processos do Scheduler
        foreach (var process in _scheduler.Processes)
        {
            _processes.Add(new ProcessViewModel(process));
        }

        ReadyProcesses = new ReadOnlyObservableCollection<ProcessViewModel>(_readyProcesses);
        BlockedProcesses = new ReadOnlyObservableCollection<ProcessViewModel>(_blockedProcesses);
        CompletedProcesses = new ReadOnlyObservableCollection<ProcessViewModel>(_completedProcesses);

        _processes.CollectionChanged += Processes_CollectionChanged;

        UpdateFilteredLists();
    }

    public void AddProcess(ProcessViewModel processViewModel)
    {
        _processes.Add(processViewModel);
        _scheduler.AddProcess(processViewModel.Model);
    }

    public async Task RunSchedulingAsync()
    {
        await _scheduler.RunAsync();
    }

    private void OnProcessUpdated(Process updatedProcess)
    {
        var processViewModel = _processes.FirstOrDefault(p => p.Id == updatedProcess.Id);
        if (processViewModel is null)
            return;

        processViewModel.UpdateFromModel();
    }

    public void Reset()
    {
        _scheduler.Reset();
        _processes.Clear();
    }

    private void Processes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateFilteredLists();
    }

    private void UpdateFilteredLists()
    {
        _readyProcesses.Clear();
        _blockedProcesses.Clear();
        _completedProcesses.Clear();

        foreach (var process in _processes)
        {
            switch (process.State)
            {
                case ProcessState.Ready:
                    _readyProcesses.Add(process);
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
        OnPropertyChanged(nameof(BlockedProcesses));
        OnPropertyChanged(nameof(CompletedProcesses));
    }
}
