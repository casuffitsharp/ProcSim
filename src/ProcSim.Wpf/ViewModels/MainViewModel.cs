using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcSim.Core;
using ProcSim.Core.Enums;
using ProcSim.Core.Models;

namespace ProcSim.Wpf.ViewModels;

public class MainViewModel : ObservableObject
{
    private readonly Scheduler _scheduler = new();
    private CancellationTokenSource _cts = new();

    public ObservableCollection<ProcessViewModel> Processes { get; private set; } = [];

    public ObservableCollection<ProcessViewModel> ReadyProcesses { get; private set; } = [];
    public ObservableCollection<ProcessViewModel> RunningProcesses { get; private set; } = [];
    public ObservableCollection<ProcessViewModel> BlockedProcesses { get; private set; } = [];
    public ObservableCollection<ProcessViewModel> CompletedProcesses { get; private set; } = [];

    public ProcessRegistrationViewModel ProcessRegistrationViewModel { get; }
    public SimulationSettingsViewModel SimulationSettingsViewModel { get; }
    public IAsyncRelayCommand RunPauseSchedulingCommand { get; }
    public IRelayCommand ResetSchedulingCommand { get; }

    public MainViewModel()
    {
        _scheduler.ProcessUpdated += OnProcessUpdated;
        _scheduler.TickUpdated += OnTickUpdated;

        ProcessRegistrationViewModel = new ProcessRegistrationViewModel(Processes);
        SimulationSettingsViewModel = new SimulationSettingsViewModel();
        RunPauseSchedulingCommand = new AsyncRelayCommand(RunPauseSchedulingAsync, CanRunPauseScheduling, AsyncRelayCommandOptions.AllowConcurrentExecutions);
        ResetSchedulingCommand = new RelayCommand(ResetScheduling, CanResetScheduling);

        Processes.CollectionChanged += Processes_CollectionChanged;

        PopulateExampleData();
    }

    private void PopulateExampleData()
    {
        Processes.Add(new(new Process(1, "P1", 10, 0, ProcessType.CpuBound)));
        Processes.Add(new(new Process(2, "P2", 5, 5, ProcessType.IoBound)));
        Processes.Add(new(new Process(3, "P3", 3, 0, ProcessType.CpuBound)));
        Processes.Add(new(new Process(4, "P4", 7, 0, ProcessType.CpuBound)));
    }

    private bool CanRunPauseScheduling()
    {
        return IsRunning || Processes.Any(p => p.State != ProcessState.Completed);
    }

    private bool CanResetScheduling() => !IsRunning;

    private async Task RunPauseSchedulingAsync()
    {
        if (IsRunning)
            await PauseSchedulingAsync();
        else
            await RunSchedulingAsync();
    }

    private async Task RunSchedulingAsync()
    {
        IsRunning = true;
        _cts = new CancellationTokenSource();
        var algorithm = SimulationSettingsViewModel.SelectedAlgorithmInstance;

        try
        {
            // Cria uma fila com os modelos de processo
            var queue = new Queue<Process>(Processes.Select(p => p.Model));
            await _scheduler.RunAsync(queue, algorithm, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Simulação pausada pelo usuário.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro inesperado: {ex.Message}");
        }
        finally
        {
            IsRunning = false;
        }

        UpdateFilteredLists();
    }

    private bool _isRunning;
    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (SetProperty(ref _isRunning, value))
            {
                SimulationSettingsViewModel.CanChangeAlgorithm = !value;
                RunPauseSchedulingCommand.NotifyCanExecuteChanged();
                ResetSchedulingCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private int _totalTimeUnits;
    public int TotalTimeUnits
    {
        get => _totalTimeUnits;
        private set => SetProperty(ref _totalTimeUnits, value);
    }

    public void AddProcess(ProcessViewModel processViewModel)
    {
        Processes.Add(processViewModel);
    }

    public async Task PauseSchedulingAsync()
    {
        _cts.Cancel();
        await Task.CompletedTask;
    }

    public void ResetScheduling()
    {
        foreach (var process in Processes)
        {
            process.Model.State = ProcessState.Ready;
            process.Model.RemainingTime = process.Model.ExecutionTime;
            process.StateHistory.Clear();
            OnPropertyChanged(nameof(process.StateHistory));
        }
        UpdateFilteredLists();
        RunPauseSchedulingCommand.NotifyCanExecuteChanged();
    }

    private void OnProcessUpdated(Process updatedProcess)
    {
        var processViewModel = Processes.FirstOrDefault(p => p.Model == updatedProcess);
        if (processViewModel is null)
            return;

        processViewModel.UpdateFromModel();
        UpdateFilteredLists();
    }

    private void OnTickUpdated()
    {
        foreach (var process in Processes)
            process.Tick();

        TotalTimeUnits = Processes.Any() ? Processes.Max(p => p.StateHistory.Count) : 0;
    }

    private void Processes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateFilteredLists();
        RunPauseSchedulingCommand.NotifyCanExecuteChanged();
    }

    private void UpdateFilteredLists()
    {
        ReadyProcesses.Clear();
        RunningProcesses.Clear();
        BlockedProcesses.Clear();
        CompletedProcesses.Clear();

        foreach (var process in Processes)
        {
            switch (process.State)
            {
                case ProcessState.Ready:
                    ReadyProcesses.Add(process);
                    break;
                case ProcessState.Running:
                    RunningProcesses.Add(process);
                    break;
                case ProcessState.Blocked:
                    BlockedProcesses.Add(process);
                    break;
                case ProcessState.Completed:
                    CompletedProcesses.Add(process);
                    break;
            }
        }

        OnPropertyChanged(nameof(ReadyProcesses));
        OnPropertyChanged(nameof(RunningProcesses));
        OnPropertyChanged(nameof(BlockedProcesses));
        OnPropertyChanged(nameof(CompletedProcesses));
        OnPropertyChanged(nameof(Processes));
    }
}
