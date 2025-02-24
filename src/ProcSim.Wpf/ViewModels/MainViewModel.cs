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

    private readonly ObservableCollection<ProcessViewModel> _processes = [];

    public ObservableCollection<ProcessViewModel> ReadyProcesses { get; private set; } = [];
    public ObservableCollection<ProcessViewModel> RunningProcesses { get; private set; } = [];
    public ObservableCollection<ProcessViewModel> BlockedProcesses { get; private set; } = [];
    public ObservableCollection<ProcessViewModel> CompletedProcesses { get; private set; } = [];

    public ProcessRegistrationViewModel ProcessRegistrationViewModel { get; }
    public SimulationSettingsViewModel SimulationSettingsViewModel { get; }
    public IAsyncRelayCommand RunSchedulingCommand { get; }
    public IAsyncRelayCommand CancelSchedulingCommand { get; }
    public IRelayCommand ResetSchedulingCommand { get; }

    public MainViewModel()
    {
        _scheduler.ProcessUpdated += OnProcessUpdated;

        ProcessRegistrationViewModel = new ProcessRegistrationViewModel(_processes);
        SimulationSettingsViewModel = new SimulationSettingsViewModel();
        RunSchedulingCommand = new AsyncRelayCommand(RunSchedulingAsync, CanRunScheduling);
        CancelSchedulingCommand = new AsyncRelayCommand(CancelSchedulingAsync, CanCancelScheduling);
        ResetSchedulingCommand = new RelayCommand(ResetScheduling, CanResetScheduling);

        _processes.CollectionChanged += Processes_CollectionChanged;
    }

    private bool CanRunScheduling()
    {
        return !IsRunning && _processes.Any(p => p.State == ProcessState.Ready);
    }

    private bool CanCancelScheduling()
    {
        return IsRunning;
    }

    private bool CanResetScheduling()
    {
        return CanRunScheduling();
    }

    private async Task RunSchedulingAsync()
    {
        IsRunning = true;

        _cts = new CancellationTokenSource();
        var algorithm = SimulationSettingsViewModel.SelectedAlgorithmInstance;

        try
        {
            await _scheduler.RunAsync(new(_processes.Select(p => p.Model)), algorithm, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Simulação cancelada pelo usuário.");
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

    public bool CanChangeAlgorithm => !IsRunning;

    private bool _isRunning;
    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (SetProperty(ref _isRunning, value))
            {
                RunSchedulingCommand.NotifyCanExecuteChanged();
                CancelSchedulingCommand.NotifyCanExecuteChanged();
                ResetSchedulingCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(CanChangeAlgorithm));
            }
        }
    }

    public void AddProcess(ProcessViewModel processViewModel)
    {
        _processes.Add(processViewModel);
    }

    public async Task CancelSchedulingAsync()
    {
        await _cts.CancelAsync();
    }

    public void ResetScheduling()
    {
        foreach (var process in _processes)
        {
            process.Model.State = ProcessState.Ready;
            process.Model.RemainingTime = process.Model.ExecutionTime;
        }

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
        RunSchedulingCommand.NotifyCanExecuteChanged();
    }

    private void UpdateFilteredLists()
    {
        ReadyProcesses.Clear();
        RunningProcesses.Clear();
        BlockedProcesses.Clear();
        CompletedProcesses.Clear();

        foreach (var process in _processes)
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
    }
}
