using ProcSim.Core.Old.Enums;
using ProcSim.Core.Old.IO;
using ProcSim.Core.Old.IO.Devices;
using ProcSim.Core.Old.Models;
using ProcSim.Core.Old.Monitoring;
using ProcSim.Core.Old.Runtime;
using ProcSim.Core.Old.Scheduling;

namespace ProcSim.Core.Old.Simulation;

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