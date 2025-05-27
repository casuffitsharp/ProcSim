using ProcSim.Core.Old.Enums;
using ProcSim.Core.Old.Factories;
using ProcSim.Core.Old.IO;
using ProcSim.Core.Old.IO.Devices;
using ProcSim.Core.Old.Logging;
using ProcSim.Core.Old.Models;
using ProcSim.Core.Old.Monitoring;
using ProcSim.Core.Old.Runtime;
using ProcSim.Core.Old.Scheduling;
using ProcSim.Core.Old.Scheduling.Algorithms;
using ProcSim.Core.Old.SystemCalls;

namespace ProcSim.Core.Old.Simulation;

public class SimulationController : ISimulationController
{
    private readonly IStructuredLogger _logger;
    private ISchedulingAlgorithm _algorithm;
    private IEnumerable<Process> _processes;
    private IEnumerable<IoDeviceConfig> _deviceConfigs;
    private int _cores;

    public SimulationController()
    {
        CancellationService = new GlobalCancellationTokenService();

        _logger = new StructuredLogger();
        TickManager = new TickManager(_logger);
        IoManager = new IoManager(_logger);
        CpuScheduler = new CpuScheduler(IoManager, _logger);
    }

    public GlobalCancellationTokenService CancellationService { get; }
    public TickManager TickManager { get; }
    public CpuScheduler CpuScheduler { get; }
    public IoManager IoManager { get; }
    public PerformanceMonitor PerformanceMonitor { get; private set; }
    public Kernel Kernel { get; private set; }
    public bool IsRunning => !TickManager.IsPaused;
    public event Action SimulationStateChanged;

    public bool HasStarted
    {
        get;
        private set
        {
            if (value != field)
            {
                field = value;
                SimulationStateChanged?.Invoke();
            }
        }
    }

    public void Initialize(IEnumerable<Process> processes, IEnumerable<IoDeviceConfig> devices, SchedulingAlgorithmType algorithmType, uint quantum, int cores)
    {
        _processes = processes;
        _deviceConfigs = devices;
        _cores = cores;

        SystemCallHandler sysCallHandler = new(IoManager);

        _algorithm = SchedulingAlgorithmFactory.Create(sysCallHandler, algorithmType);
        if (_algorithm is IPreemptiveAlgorithm pa)
            pa.Quantum = quantum;

        BuildKernel();
    }

    private void BuildKernel()
    {
        TickManager.Pause();

        CpuScheduler.ClearQueue();
        IoManager.Reset();

        Kernel = new Kernel(TickManager, CpuScheduler, _algorithm, _cores);
        PerformanceMonitor = new PerformanceMonitor(Kernel, _logger);

        HasStarted = false;
    }

    public async Task StartAsync()
    {
        if (HasStarted)
        {
            TickManager.Resume();
            PerformanceMonitor.Resume();
        }
        else
        {
            if (Kernel == null)
                BuildKernel();

            Kernel.ClearProcesses();
            foreach (Process p in _processes)
                Kernel.RegisterProcess(p);

            IoManager.Configure(_deviceConfigs, TickManager.DelayFunc, CancellationService.CurrentToken);

            HasStarted = true;

            try { await Kernel.RunAsync(CancellationService.TokenProvider); }
            catch (OperationCanceledException) { /*swallow*/}

            Pause();
        }
    }

    public void Pause()
    {
        TickManager.Pause();
        PerformanceMonitor.Pause();
    }

    public async Task ResetAsync()
    {
        await CancellationService.ResetAsync();
        TickManager.Pause();
        CpuScheduler.ClearQueue();
        IoManager.Reset();
        Kernel.ClearProcesses();
        PerformanceMonitor.Reset();

        Kernel = null;
        HasStarted = false;
    }
}
