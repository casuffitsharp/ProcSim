using ProcSim.Core.Enums;
using ProcSim.Core.IO;
using ProcSim.Core.IO.Devices;
using ProcSim.Core.Models;
using ProcSim.Core.Monitoring;
using ProcSim.Core.Runtime;
using ProcSim.Core.Scheduling;

namespace ProcSim.Core.Simulation;

public interface ISimulationController
{
    Task StartAsync();
    void Pause();
    Task ResetAsync();
    void Initialize(IEnumerable<Process> processes, IEnumerable<IoDeviceConfig> devices, SchedulingAlgorithmType algorithmType, uint quantum, int cores);

    PerformanceMonitor PerformanceMonitor { get; }
    TickManager TickManager { get; }
    CpuScheduler CpuScheduler { get; }
    IoManager IoManager { get; }
    Kernel Kernel { get; }
    GlobalCancellationTokenService CancellationService { get; }
    bool HasStarted { get; }
    bool IsRunning { get; }

    event Action SimulationStateChanged;
}