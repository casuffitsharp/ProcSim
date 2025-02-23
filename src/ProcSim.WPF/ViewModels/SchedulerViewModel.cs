using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ProcSim.Core;
using ProcSim.Core.Enums;
using ProcSim.Core.Models;

namespace ProcSim.WPF.ViewModels;

public class SchedulerViewModel : ViewModelBase
{
    private readonly Scheduler _scheduler = new();
    private readonly ObservableCollection<ProcessViewModel> _processes = [];

    public ObservableCollection<ProcessViewModel> ReadyProcesses { get; private set; } = [];
    public ObservableCollection<ProcessViewModel> BlockedProcesses { get; private set; } = [];
    public ObservableCollection<ProcessViewModel> CompletedProcesses { get; private set; } = [];

    public SchedulerViewModel()
    {
        _scheduler.ProcessUpdated += OnProcessUpdated;

        // Carrega os processos do Scheduler
        foreach (var process in _scheduler.Processes)
        {
            _processes.Add(new ProcessViewModel(process));
        }

        UpdateFilteredLists();
    }

    public void AddProcess(ProcessViewModel processViewModel)
    {
        _processes.Add(processViewModel);
        _scheduler.AddProcess(processViewModel.Model);
        UpdateFilteredLists();
    }

    public async Task RunSchedulingAsync()
    {
        await _scheduler.RunAsync();
        UpdateFilteredLists();
    }

    private void OnProcessUpdated(Process updatedProcess)
    {
        var processViewModel = _processes.FirstOrDefault(p => p.Id == updatedProcess.Id);
        if (processViewModel is null)
            return;

        processViewModel.NotifyPropertyChanged(nameof(processViewModel.RemainingTime));
        processViewModel.NotifyPropertyChanged(nameof(processViewModel.State));

        UpdateFilteredLists();
    }

    public void Reset()
    {
        _scheduler.Reset();
        _processes.Clear();
        UpdateFilteredLists();
    }

    private void UpdateFilteredLists()
    {
        ReadyProcesses = [.. _processes.Where(p => p.State == ProcessState.Ready)];
        BlockedProcesses = [.. _processes.Where(p => p.State == ProcessState.Blocked)];
        CompletedProcesses = [.. _processes.Where(p => p.State == ProcessState.Completed)];

        OnPropertyChanged(nameof(ReadyProcesses));
        OnPropertyChanged(nameof(BlockedProcesses));
        OnPropertyChanged(nameof(CompletedProcesses));
    }
}
