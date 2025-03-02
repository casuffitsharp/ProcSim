using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Standard;
using ProcSim.Core;
using ProcSim.Core.Enums;
using ProcSim.Core.Models;
using ProcSim.Core.Scheduling;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ProcSim.Wpf.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly Scheduler _scheduler;
    private readonly TickManager _tickManager = new();
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
        CpuTime = _tickManager.CpuTime;

        _tickManager.TickOccurred += OnTickOcurred;
        _tickManager.RunStateChanged += () => IsRunning = !_tickManager.IsPaused;
        _scheduler = new(_tickManager);
        _scheduler.ProcessUpdated += OnProcessUpdated;

        ProcessRegistrationViewModel = new ProcessRegistrationViewModel(Processes);
        SimulationSettingsViewModel = new SimulationSettingsViewModel();
        RunPauseSchedulingCommand = new AsyncRelayCommand(RunPauseSchedulingAsync, CanRunPauseScheduling, AsyncRelayCommandOptions.AllowConcurrentExecutions);
        ResetSchedulingCommand = new RelayCommand(ResetScheduling, CanResetScheduling);

        Processes.CollectionChanged += Processes_CollectionChanged;

        PopulateExampleData();
    }

    private void PopulateExampleData()
    {
        Processes.Add(new(new Process(1, "P1", 5, 0, ProcessType.CpuBound)));
        Processes.Add(new(new Process(2, "P2", 5, 5, ProcessType.IoBound)));
        Processes.Add(new(new Process(3, "P3", 3, 0, ProcessType.CpuBound)));
        Processes.Add(new(new Process(4, "P4", 7, 0, ProcessType.CpuBound)));
    }

    private bool CanRunPauseScheduling()
    {
        return IsRunning || Processes.Any(p => p.State != ProcessState.Completed);
    }

    private bool CanResetScheduling()
    {
        return _tickManager.IsPaused;
    }

    private async Task RunPauseSchedulingAsync()
    {
        if (IsRunning)
        {
            PauseScheduling();
        }
        else
        {
            await RunSchedulingAsync();
        }
    }

    private async Task RunSchedulingAsync()
    {
        _cts = new CancellationTokenSource();
        ISchedulingAlgorithm algorithm = SimulationSettingsViewModel.SelectedAlgorithmInstance;

        try
        {
            Queue<Process> queue = new(Processes.Select(p => p.Model));
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

        UpdateFilteredLists();
    }

    public bool IsRunning
    {
        get => field;
        private set
        {
            if (SetProperty(ref field, value))
            {
                SimulationSettingsViewModel.CanChangeAlgorithm = !value;
                RunPauseSchedulingCommand.NotifyCanExecuteChanged();
                ResetSchedulingCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public ushort CpuTime
    {
        get => field;
        set
        {
            if (SetProperty(ref field, value))
            {
                _tickManager.CpuTime = value;
                OnPropertyChanged(nameof(CpuTimeTs));
            }
        }
    }

    public TimeSpan CpuTimeTs => TimeSpan.FromMilliseconds(CpuTime);

    [ObservableProperty]
    public partial int TotalTimeUnits { get; set; }

    public void AddProcess(ProcessViewModel processViewModel)
    {
        Processes.Add(processViewModel);
    }

    public void PauseScheduling()
    {
        _tickManager.Pause();
    }

    public void ResetScheduling()
    {
        foreach (ProcessViewModel process in Processes)
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
        ProcessViewModel processViewModel = Processes.FirstOrDefault(p => p.Model == updatedProcess);
        if (processViewModel is null)
            return;

        processViewModel.UpdateFromModel();
        UpdateFilteredLists();
    }

    private void OnTickOcurred()
    {
        foreach (ProcessViewModel process in Processes)
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

        foreach (ProcessViewModel process in Processes)
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
