using ProcSim.Core.IO;
using ProcSim.Core.Monitoring.Models;
using ProcSim.Core.Process;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ProcSim.Core.Monitoring;

public class MonitoringService : IDisposable
{
    private const int DefaultIntervalMs = 1000;
    private const bool DebugEnabled = false;

    private readonly PeriodicTimer _timer;
    private readonly CancellationTokenSource _cts = new();

    private readonly Dictionary<uint, CpuSnapshot> _prevCpu = [];
    private ulong _prevGlobalCycle;
    private readonly Dictionary<int, ulong> _prevProcCycles = [];
    private readonly Dictionary<uint, ulong> _prevInterrupt = [];
    private readonly Dictionary<DeviceChannelKey, ChannelSnapshot> _prevChan = [];

    private readonly ConcurrentDictionary<DeviceChannelKey, ChannelStats> _channelStats = [];
    private readonly ConcurrentDictionary<IoRequestNotification, ulong> _ioStarts = [];

    private Kernel _kernel;

    public MonitoringService()
    {
        _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(DefaultIntervalMs));
        Task.Run(() => CollectAsync(_cts.Token));
    }

    public event Action<IReadOnlyList<ProcessSnapshot>> ProcessListUpdated;
    public event Action OnReset;

    public ConcurrentDictionary<uint, List<CpuCoreUsageMetric>> CpuCoreMetrics { get; } = [];
    public List<CpuTotalUsageMetric> CpuTotalMetrics { get; } = [];
    public ConcurrentDictionary<int, List<ProcessCpuUsageMetric>> ProcessMetrics { get; } = [];
    public ConcurrentDictionary<DeviceChannelKey, List<IoChannelUsageMetric>> IoChannelMetrics { get; } = [];
    public ConcurrentDictionary<uint, List<DeviceAggregateUsageMetric>> DeviceAggregateMetrics { get; } = [];
    public ConcurrentDictionary<(uint DeviceId, uint ChannelId, int ProcessId), List<ProcessIoMetric>> ProcessIoMetrics { get; } = [];


    public void Resume()
    {
        _timer.Period = TimeSpan.FromMilliseconds(DefaultIntervalMs);
    }

    public void Pause()
    {
        _timer.Period = Timeout.InfiniteTimeSpan;
    }

    public void SetKernel(Kernel kernel)
    {
        _kernel = kernel;
        OnReset?.Invoke();
        if (_kernel is null)
        {
            Debug.WriteLineIf(DebugEnabled, "[MonitoringService] Kernel set to null.");
            return;
        }

        _prevCpu.Clear();
        _prevProcCycles.Clear();
        _channelStats.Clear();
        _prevChan.Clear();

        // inicializa snapshots de CPU e processos
        foreach (CPU cpu in _kernel.Cpus.Values)
            _prevCpu[cpu.Id] = new CpuSnapshot(cpu);

        _prevGlobalCycle = kernel.GlobalCycle;

        foreach (PCB pcb in kernel.Programs.Keys)
            _prevProcCycles[pcb.ProcessId] = pcb.CpuCycles;

        // configura estatísticas por canal e subscreve eventos
        foreach (IODevice device in kernel.Devices.Values)
        {
            for (uint channel = 0; channel < device.Channels; channel++)
            {
                DeviceChannelKey key = new(device.Id, channel);
                _channelStats[key] = new ChannelStats();
                _prevChan[key] = new ChannelSnapshot(0, 0);
            }
            SubscribeDevice(device);
        }

        Debug.WriteLineIf(DebugEnabled, $"[MonitoringService] Kernel set with {kernel.Cpus.Count} CPUs and {kernel.Devices.Count} devices.");
    }

    private async Task CollectAsync(CancellationToken ct)
    {
        while (await _timer.WaitForNextTickAsync(ct))
        {
            if (_kernel is null || _kernel.GlobalCycle == 0)
            {
                Debug.WriteLineIf(DebugEnabled, "[MonitoringService] Kernel not set or GlobalCycle is zero. Skipping collection.");
                continue;
            }

            DateTime ts = DateTime.UtcNow;
            ulong nowTick = _kernel.GlobalCycle;
            ulong intervalCycles = nowTick - _prevGlobalCycle;
            _prevGlobalCycle = nowTick;

            // 1) CPU por núcleo e agregados
            ulong totalC = 0UL;      // total de ticks em todas as CPUs
            ulong totalU = 0UL;      // ticks de usuário (sum of dU)
            ulong totalS = 0UL;      // ticks de syscall (sum of dS)
            ulong totalI = 0UL;      // ticks de interrupção (sum of dI)
            ulong totalIdle = 0UL;   // ticks de idle (sum of dIdle)

            foreach (CPU cpu in _kernel.Cpus.Values)
            {
                CpuSnapshot prev = _prevCpu[cpu.Id];

                // Ciclos totais que avançaram no core
                ulong dCyclesTotal = cpu.CycleCount - prev.CycleCount;
                // Ciclos de usuário que avançaram no core
                ulong dUserCycles = cpu.UserCycleCount - prev.UserCycleCount;
                // Ciclos de syscall que avançaram no core
                ulong dSyscallCycles = cpu.SyscallCycleCount - prev.SyscallCycleCount;
                // Ciclos de interrupção que avançaram no core
                ulong dInterruptCycles = cpu.InterruptCycleCount - prev.InterruptCycleCount;
                ulong dIdleCycles = cpu.IdleCycleCount - prev.IdleCycleCount;

                _prevCpu[cpu.Id] = new CpuSnapshot(cpu);

                totalC += dCyclesTotal;
                totalU += dUserCycles;
                totalS += dSyscallCycles;
                totalI += dInterruptCycles;
                totalIdle += dIdleCycles;

                CpuCoreMetrics.AddOrUpdate(cpu.Id, _ => [new(ts, cpu.Id, dCyclesTotal, dUserCycles, dSyscallCycles, dInterruptCycles, dIdleCycles)], (_, lst) => { lst.Add(new(ts, cpu.Id, dCyclesTotal, dUserCycles, dSyscallCycles, dInterruptCycles, dIdleCycles)); return lst; });
            }

            CpuTotalMetrics.Add(new CpuTotalUsageMetric(ts, totalC, totalU, totalS, totalI, totalIdle));

            ushort interruptsPercent = 0;
            if (totalC > 0UL)
                interruptsPercent = (ushort)Math.Min(Math.Round(100.0 * totalI / totalC), 100.0);

            List<ProcessSnapshot> processSnapshots = [];
            processSnapshots.Add(new(Pid: -1, Name: "System Interrupts", State: ProcessState.Running, CpuUsage: interruptsPercent, StaticPriority: ProcessStaticPriority.Normal, DynamicPriority: -1));

            ushort idlePercent = 0;
            if (totalC > 0UL)
                idlePercent = (ushort)Math.Min(Math.Round(100.0 * totalIdle / totalC), 100.0);

            processSnapshots.Add(new ProcessSnapshot(Pid: 0, Name: "Idle", State: idlePercent > 0 ? ProcessState.Running : ProcessState.Ready, CpuUsage: idlePercent, StaticPriority: ProcessStaticPriority.Normal, DynamicPriority: -1));

            foreach (PCB pcb in _kernel.Programs.Keys)
            {
                if (pcb.ProcessId < _kernel.Cpus.Count)
                    continue;

                ulong curr = pcb.UserCycles + pcb.SyscallCycles;

                if (!_prevProcCycles.TryGetValue(pcb.ProcessId, out ulong prev))
                {
                    _prevProcCycles[pcb.ProcessId] = curr;
                    continue;
                }

                ulong deltaProc = (curr > prev) ? (curr - prev) : 0UL;
                _prevProcCycles[pcb.ProcessId] = curr;

                ushort procUsage = 0;
                if (deltaProc > 0 && totalC > 0UL)
                    procUsage = (ushort)Math.Min(Math.Round(100.0 * deltaProc / totalC), 100.0);

                processSnapshots.Add(new ProcessSnapshot(pcb.ProcessId, pcb.Name, pcb.State, procUsage, pcb.StaticPriority, pcb.DynamicPriority));
                ProcessMetrics.AddOrUpdate(pcb.ProcessId, _ => [new(ts, pcb.ProcessId, procUsage)], (_, lst) => { lst.Add(new(ts, pcb.ProcessId, procUsage)); return lst; });
            }

            ProcessListUpdated?.Invoke(processSnapshots);
        }
    }

    private void SubscribeDevice(IODevice device)
    {
        device.IORequestStarted += (req) =>
        {
            _ioStarts[req] = _kernel.GlobalCycle;
            Debug.WriteLineIf(DebugEnabled, $"[MonitoringService] IORequestStarted: Device={req.DeviceId}, Channel={req.Channel}, Pid={req.Pid}, Cycle={_kernel.GlobalCycle}");
        };

        device.IORequestCompleted += (req) =>
        {
            if (!_ioStarts.TryRemove(req, out ulong t0))
            {
                Debug.WriteLineIf(DebugEnabled, $"[MonitoringService] IORequestCompleted: Device={req.DeviceId}, Channel={req.Channel}, Pid={req.Pid} - Start not found!");
                return;
            }

            ulong duration = _kernel.GlobalCycle - t0;
            AddProcessIoMetric(req.DeviceId, req.Channel, req.Pid, duration);

            // atualiza stats do canal
            DeviceChannelKey key = new(req.DeviceId, req.Channel);
            ChannelStats stats = _channelStats[key];
            stats.TotalRequests++;
            stats.BusyCycles += duration;

            Debug.WriteLineIf(DebugEnabled, $"[MonitoringService] IORequestCompleted: Device={req.DeviceId}, Channel={req.Channel}, Pid={req.Pid}, Duration={duration} cycles");
        };
    }

    private void AddProcessIoMetric(uint deviceId, uint channelId, int processId, ulong latencyCycles)
    {
        ProcessIoMetric metric = new(
            Timestamp: DateTime.UtcNow,
            DeviceId: deviceId,
            ChannelId: channelId,
            ProcessId: processId,
            LatencyCycles: latencyCycles
        );

        (uint deviceId, uint channelId, int processId) key = (deviceId, channelId, processId);
        ProcessIoMetrics.AddOrUpdate(key, _ => [metric], (_, lst) => { lst.Add(metric); return lst; });

        Debug.WriteLineIf(DebugEnabled, $"[MonitoringService] ProcessIoMetric: Device={deviceId}, Channel={channelId}, Process={processId}, Latency={latencyCycles} cycles");
    }

    public void Dispose()
    {
        _cts.Cancel();
        _timer.Dispose();
        Debug.WriteLineIf(DebugEnabled, "[MonitoringService] Disposed");
    }

    private record CpuSnapshot(ulong CycleCount, ulong UserCycleCount, ulong SyscallCycleCount, ulong InterruptCycleCount, ulong IdleCycleCount)
    {
        public CpuSnapshot(CPU cpu) : this(cpu.CycleCount, cpu.UserCycleCount, cpu.SyscallCycleCount, cpu.InterruptCycleCount, cpu.IdleCycleCount) { }
    }

    private record ChannelSnapshot(long TotalRequests, ulong BusyCycles)
    {
        public ChannelSnapshot(ChannelStats stats) : this(stats.TotalRequests, stats.BusyCycles) { }
    }

    private class ChannelStats
    {
        public long TotalRequests { get; set; }
        public ulong BusyCycles { get; set; }
    }
}
