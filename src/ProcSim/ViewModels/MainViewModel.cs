using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcSim.Core.Enums;
using ProcSim.Core.Logging;
using ProcSim.Core.Models;
using ProcSim.Core.Runtime;
using ProcSim.Core.IO;
using ProcSim.Core.SystemCalls;
using ProcSim.Core.Scheduling;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ProcSim.Core.Models.Operations;
using ProcSim.Core.IO.Devices;

namespace ProcSim.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ILogger _logger;
    private readonly TickManager _tickManager;
    private readonly IoManager _ioManager;
    private readonly CpuScheduler _cpuScheduler;
    private readonly ISysCallHandler _sysCallHandler;
    private readonly Kernel _kernel;
    private CancellationTokenSource _cts = new();

    public ObservableCollection<ProcessViewModel> Processes { get; private set; } = [];
    public ObservableCollection<ProcessViewModel> ReadyProcesses { get; private set; } = [];
    public ObservableCollection<ProcessViewModel> RunningProcesses { get; private set; } = [];
    public ObservableCollection<ProcessViewModel> BlockedProcesses { get; private set; } = [];
    public ObservableCollection<ProcessViewModel> CompletedProcesses { get; private set; } = [];

    public ProcessesViewModel ProcessesViewModel { get; }
    public SimulationSettingsViewModel SimulationSettingsViewModel { get; }
    public IAsyncRelayCommand RunPauseSchedulingCommand { get; }
    public IRelayCommand ResetSchedulingCommand { get; }

    public MainViewModel()
    {
        _logger = new StructuredLogger();

        _tickManager = new TickManager(_logger)
        {
            CpuTime = 10 // Valor padrão, que pode ser alterado via binding
        };

        _ioManager = new IoManager(_logger);
        //_ioManager.AddDevice(seuDispositivo);

        _cpuScheduler = new CpuScheduler(_ioManager, _logger);
        _sysCallHandler = new SystemCallHandler(_ioManager);

        SimulationSettingsViewModel = new SimulationSettingsViewModel();
        var schedulingAlgorithm = SimulationSettingsViewModel.SelectedAlgorithmInstance;

        _kernel = new Kernel(_tickManager, _cpuScheduler, _sysCallHandler, schedulingAlgorithm);

        _tickManager.RunStateChanged += () => IsRunning = !_tickManager.IsPaused;

        PopulateExampleData();
        ProcessesViewModel = new ProcessesViewModel(Processes);
        RunPauseSchedulingCommand = new AsyncRelayCommand(RunPauseSchedulingAsync, CanRunPauseScheduling, AsyncRelayCommandOptions.AllowConcurrentExecutions);
        ResetSchedulingCommand = new RelayCommand(ResetScheduling, CanResetScheduling);

        Processes.CollectionChanged += Processes_CollectionChanged;

        CpuTime = _tickManager.CpuTime;
    }

    private void PopulateExampleData()
    {
        Processes.Add(new ProcessViewModel(new Process(1, "P1", [
            new CpuOperation(5),
            new IoOperation(10, IoDeviceType.Disk),
            new CpuOperation(3),
            new IoOperation(5, IoDeviceType.Disk),
            new CpuOperation(2)
        ])));
        Processes.Add(new ProcessViewModel(new Process(2, "P2", [
            new CpuOperation(5),
            new CpuOperation(3),
            new CpuOperation(30),
            new CpuOperation(10),
            new IoOperation(5, IoDeviceType.Disk),
            new CpuOperation(2)
        ])));
        Processes.Add(new ProcessViewModel(new Process(3, "P3", [
            new IoOperation(5, IoDeviceType.Disk),
            new IoOperation(10, IoDeviceType.Disk),
            new CpuOperation(3),
            new IoOperation(5, IoDeviceType.Disk),
            new CpuOperation(2)
        ])));
        Processes.Add(new ProcessViewModel(new Process(4, "P4", [
            new CpuOperation(5),
            new CpuOperation(3),
            new CpuOperation(2)
        ])));
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

        var schedulingAlgorithm = SimulationSettingsViewModel.SelectedAlgorithmInstance;
        _kernel.SchedulingAlgorithm = schedulingAlgorithm;

        try
        {
            await _kernel.RunAsync(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.Log("Simulação pausada pelo usuário.");
        }
        catch (Exception ex)
        {
            _logger.Log($"Erro inesperado: {ex.Message}");
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
            process.Model.Reset();

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
