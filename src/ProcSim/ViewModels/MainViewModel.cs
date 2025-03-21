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
using ProcSim.Core.Simulation;
using System.ComponentModel;
using System.IO;

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

        VmSettingsVm = new VmSettingsViewModel(new VmConfigRepository());
        VmSettingsVm.PropertyChanged += VmSettingsVm_PropertyChanged;
        var schedulingAlgorithm = VmSettingsVm.SelectedAlgorithmInstance;

        _kernel = new Kernel(_tickManager, _cpuScheduler, _sysCallHandler, schedulingAlgorithm);

        _tickManager.RunStateChanged += () => IsRunning = !_tickManager.IsPaused;

        ProcessesSettingsVm = new ProcessesSettingsViewModel(new ProcessesConfigRepository());
        ProcessesSettingsVm.PropertyChanged += ProcessesSettingsVm_PropertyChanged;
        RunPauseSchedulingCommand = new AsyncRelayCommand(RunPauseSchedulingAsync, CanRunPauseScheduling, AsyncRelayCommandOptions.AllowConcurrentExecutions);
        ResetSchedulingCommand = new RelayCommand(ResetScheduling, CanResetScheduling);

        Processes.CollectionChanged += Processes_CollectionChanged;

        CpuTime = _tickManager.CpuTime;
    }

    public ObservableCollection<ProcessViewModel> Processes { get; private set; } = [];
    public ObservableCollection<ProcessViewModel> ReadyProcesses { get; private set; } = [];
    public ObservableCollection<ProcessViewModel> RunningProcesses { get; private set; } = [];
    public ObservableCollection<ProcessViewModel> BlockedProcesses { get; private set; } = [];
    public ObservableCollection<ProcessViewModel> CompletedProcesses { get; private set; } = [];

    public ProcessesSettingsViewModel ProcessesSettingsVm { get; }
    public VmSettingsViewModel VmSettingsVm { get; }

    public IAsyncRelayCommand RunPauseSchedulingCommand { get; }
    public IRelayCommand ResetSchedulingCommand { get; }

    [ObservableProperty]
    public partial int TotalTimeUnits { get; set; }

    public string StatusBarMessage
    {
        get
        {
            return $"VM: {Path.GetFileNameWithoutExtension(VmSettingsVm.CurrentFile ?? "New")} | Processos: {Path.GetFileNameWithoutExtension(ProcessesSettingsVm.CurrentFile ?? "New")}";
        }
    }

    public bool IsRunning
    {
        get => field;
        private set
        {
            if (SetProperty(ref field, value))
            {
                VmSettingsVm.CanChangeAlgorithm = !value;
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

        var schedulingAlgorithm = VmSettingsVm.SelectedAlgorithmInstance;
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

    private void VmSettingsVm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(VmSettingsVm.CurrentFile))
            OnPropertyChanged(nameof(StatusBarMessage));
    }

    private void ProcessesSettingsVm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProcessesSettingsVm.CurrentFile))
            OnPropertyChanged(nameof(StatusBarMessage));
    }
}
