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

    private volatile KernelState _state;
    private bool _disposed;

    public MonitoringService()
    {
        _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(DefaultIntervalMs));
        Task.Run(() => CollectAsync(_cts.Token));
    }

    public event Action<IReadOnlyList<ProcessSnapshot>> ProcessListUpdated;
    public event Action OnMetricsUpdated;
    public event Action OnReset;

    public ConcurrentDictionary<uint, List<CpuUsageMetric>> CpuCoreMetrics { get; } = [];
    public List<CpuUsageMetric> CpuTotalMetrics { get; } = [];
    public ConcurrentDictionary<int, List<ProcessUsageMetric>> ProcessMetrics { get; } = [];
    public ConcurrentDictionary<uint, List<DeviceUsageMetric>> DeviceMetrics { get; } = [];


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
        KernelState newState = null;

        if (kernel is not null)
        {
            newState = new KernelState(kernel);

            // inicializa snapshots de CPU e processos
            foreach (Cpu cpu in kernel.Cpus.Values)
                newState.PrevCpu[cpu.Id] = new CpuSnapshot(cpu);

            foreach (Pcb pcb in kernel.Programs.Keys)
                newState.PrevProcCycles[pcb.ProcessId] = 0;

            foreach (IODevice device in kernel.Devices.Values)
            {
                ConcurrentDictionary<uint, DeviceChannelStats> channelStats = [];
                Dictionary<uint, ChannelSnapshot> channelSnapshots = [];
                ConcurrentDictionary<uint, bool> channelBusy = [];
                newState.DeviceStats[device.Id] = channelStats;
                newState.PrevChan[device.Id] = channelSnapshots;
                newState.DeviceChannelBusy[device.Id] = channelBusy;

                for (uint channel = 0; channel < device.Channels; channel++)
                {
                    channelStats[channel] = new DeviceChannelStats();
                    channelSnapshots[channel] = new ChannelSnapshot();
                }

                SubscribeDevice(device, newState);
            }

            Debug.WriteLineIf(DebugEnabled, $"[MonitoringService] Kernel set with {kernel.Cpus.Count} CPUs and {kernel.Devices.Count} devices.");
        }
        else
        {
            Debug.WriteLineIf(DebugEnabled, "[MonitoringService] Kernel set to null.");
        }

        _state = newState;

        CpuCoreMetrics.Clear();
        CpuTotalMetrics.Clear();
        ProcessMetrics.Clear();
        DeviceMetrics.Clear();

        OnReset?.Invoke();
    }

    private async Task CollectAsync(CancellationToken ct)
    {
        while (await _timer.WaitForNextTickAsync(ct))
        {
            KernelState state = _state;

            if (state is null || state.Kernel.GlobalCycle == 0)
            {
                Debug.WriteLineIf(DebugEnabled, "[MonitoringService] Kernel not set or GlobalCycle is zero. Skipping collection.");
                continue;
            }

            DateTime ts = DateTime.UtcNow;
            ulong nowTick = state.Kernel.GlobalCycle;

            CpuTotals cpuTotals = CollectCpuMetrics(ts, state);
            List<ProcessSnapshot> processSnapshots = CollectProcessMetrics(ts, cpuTotals, state);
            CollectDeviceMetrics(ts, nowTick, state);

            ProcessListUpdated?.Invoke(processSnapshots);
            OnMetricsUpdated?.Invoke();
        }
    }

    private CpuTotals CollectCpuMetrics(DateTime ts, KernelState state)
    {            
        // CPU por núcleo e agregados
        ulong totalC = 0UL;      // total de ticks em todas as CPUs
        ulong totalU = 0UL;      // ticks de usuário (sum of dU)
        ulong totalS = 0UL;      // ticks de syscall (sum of dS)
        ulong totalI = 0UL;      // ticks de interrupção (sum of dI)
        ulong totalIdle = 0UL;   // ticks de idle (sum of dIdle)

        foreach (Cpu cpu in state.Kernel.Cpus.Values)
        {
            CpuSnapshot prev = state.PrevCpu[cpu.Id];

            ulong dCyclesTotal = cpu.CycleCount - prev.CycleCount;
            ulong dUserCycles = cpu.UserCycleCount - prev.UserCycleCount;
            ulong dSyscallCycles = cpu.SyscallCycleCount - prev.SyscallCycleCount;
            ulong dInterruptCycles = cpu.InterruptCycleCount - prev.InterruptCycleCount;
            ulong dIdleCycles = cpu.IdleCycleCount - prev.IdleCycleCount;

            state.PrevCpu[cpu.Id] = new CpuSnapshot(cpu);

            totalC += dCyclesTotal;
            totalU += dUserCycles;
            totalS += dSyscallCycles;
            totalI += dInterruptCycles;
            totalIdle += dIdleCycles;

            CpuCoreMetrics.AddOrUpdate(cpu.Id, _ => [new(ts, dCyclesTotal, dUserCycles, dSyscallCycles, dInterruptCycles, dIdleCycles)], (_, lst) => { lst.Add(new(ts, dCyclesTotal, dUserCycles, dSyscallCycles, dInterruptCycles, dIdleCycles)); return lst; });
        }

        CpuTotalMetrics.Add(new(ts, totalC, totalU, totalS, totalI, totalIdle));

        return new CpuTotals(totalC, totalU, totalS, totalI, totalIdle);
    }

    private List<ProcessSnapshot> CollectProcessMetrics(DateTime ts, CpuTotals cpuTotals, KernelState state)
    {
        List<ProcessSnapshot> processSnapshots = [];

        ushort interruptsPercent = CalculatePercentage(cpuTotals.TotalInterrupt, cpuTotals.TotalCycles);
        processSnapshots.Add(new(Pid: -1, Name: "System Interrupts", State: ProcessState.Running, CpuUsage: interruptsPercent, StaticPriority: ProcessStaticPriority.Normal, DynamicPriority: -1));

        ushort idlePercent = CalculatePercentage(cpuTotals.TotalIdle, cpuTotals.TotalCycles);
        processSnapshots.Add(new ProcessSnapshot(Pid: 0, Name: "Idle", State: idlePercent > 0 ? ProcessState.Running : ProcessState.Ready, CpuUsage: idlePercent, StaticPriority: ProcessStaticPriority.Normal, DynamicPriority: -1));

        foreach (Pcb pcb in state.Kernel.Programs.Keys)
        {
            if (pcb.ProcessId < state.Kernel.Cpus.Count)
                continue;

            ulong currentCycles = pcb.UserCycles + pcb.SyscallCycles;

            if (!state.PrevProcCycles.TryGetValue(pcb.ProcessId, out ulong prev))
            {
                state.PrevProcCycles[pcb.ProcessId] = currentCycles;
                continue;
            }

            ulong deltaProc = (currentCycles > prev) ? (currentCycles - prev) : 0UL;
            state.PrevProcCycles[pcb.ProcessId] = currentCycles;

            ushort procUsage = 0;
            if (deltaProc > 0 && cpuTotals.TotalCycles > 0UL)
                procUsage = CalculatePercentage(deltaProc, cpuTotals.TotalCycles);

            processSnapshots.Add(new ProcessSnapshot(pcb.ProcessId, pcb.Name, pcb.State, procUsage, pcb.StaticPriority, pcb.DynamicPriority));
            ProcessUsageMetric processMetric = new()
            {
                CpuTime = currentCycles,
                DynamicPriority = pcb.DynamicPriority,
                StaticPriority = pcb.StaticPriority,
                State = pcb.State,
                Timestamp = ts,
                IoTime = pcb.WaitCycles
            };
            ProcessMetrics.AddOrUpdate(pcb.ProcessId, _ => [processMetric], (_, lst) => { lst.Add(processMetric); return lst; });
        }

        return processSnapshots;
    }

    private void CollectDeviceMetrics(DateTime ts, ulong nowTick, KernelState state)
    {
        foreach ((uint deviceId, ConcurrentDictionary<uint, DeviceChannelStats> channelsStats) in state.DeviceStats)
        {
            state.PrevChan.TryGetValue(deviceId, out Dictionary<uint, ChannelSnapshot> prevChannelsSnapshots);

            DeviceUsageMetric deviceMetric = new()
            {
                Timestamp = ts,
                ChannelsMetrics = []
            };

            foreach ((uint channel, DeviceChannelStats channelStats) in channelsStats)
            {
                IoChannelUsageMetric channelMetric = CollectChannelMetric(deviceId, channel, channelStats, prevChannelsSnapshots, nowTick, state);
                deviceMetric.ChannelsMetrics[channel] = channelMetric;

                deviceMetric.RequestsDelta += channelMetric.RequestsDelta;
                deviceMetric.BusyDelta += channelMetric.BusyDelta;
                deviceMetric.CyclesDelta += channelMetric.CyclesDelta;
            }

            DeviceMetrics.AddOrUpdate(deviceId, _ => [deviceMetric], (_, lst) => { lst.Add(deviceMetric); return lst; });
        }
    }

    private static IoChannelUsageMetric CollectChannelMetric(uint deviceId, uint channel, DeviceChannelStats channelStats, Dictionary<uint, ChannelSnapshot> prevChannelsSnapshots, ulong nowTick, KernelState state)
    {
        ulong totalRequests = channelStats.TotalRequests;
        ulong busyCycles = channelStats.BusyCycles;
        ChannelSnapshot snapshot = new(totalRequests, busyCycles, nowTick);

        prevChannelsSnapshots.TryGetValue(channel, out ChannelSnapshot prevSnapshot);
        prevSnapshot ??= new();
        prevChannelsSnapshots[channel] = snapshot;

        ulong dRequests = totalRequests - prevSnapshot.TotalRequests;
        ulong dCycles = nowTick - prevSnapshot.TotalCycles;
        ulong dBusyCycles = Math.Min(busyCycles - prevSnapshot.BusyCycles, dCycles);

        if (dBusyCycles == 0)
        {
            bool isBusy = state.DeviceChannelBusy[deviceId].TryGetValue(channel, out bool busy) && busy;
            if (isBusy)
                dBusyCycles = dCycles;
        }

        return new IoChannelUsageMetric(dRequests, dBusyCycles, dCycles);
    }

    private static ushort CalculatePercentage(ulong value, ulong total)
    {
        if (total == 0UL)
            return 0;

        return (ushort)Math.Min(Math.Round(100.0 * value / total), 100.0);
    }

    private static void SubscribeDevice(IODevice device, KernelState state)
    {
        device.IORequestStarted += (req) =>
        {
            state.IoStarts[req] = state.Kernel.GlobalCycle;
            state.DeviceChannelBusy[req.DeviceId][req.Channel] = true;
            Debug.WriteLineIf(DebugEnabled, $"[MonitoringService] IORequestStarted: Device={req.DeviceId}, Channel={req.Channel}, Pid={req.Pid}, Cycle={state.Kernel.GlobalCycle}");
        };

        device.IORequestCompleted += (req) => OnIoRequestCompleted(req, state);
    }

    private static void OnIoRequestCompleted(IoRequestNotification req, KernelState state)
    {
        if (!state.IoStarts.TryRemove(req, out ulong t0))
        {
            Debug.WriteLineIf(DebugEnabled, $"[MonitoringService] IORequestCompleted: Device={req.DeviceId}, Channel={req.Channel}, Pid={req.Pid} - Start not found!");
            return;
        }

        state.DeviceChannelBusy[req.DeviceId][req.Channel] = false;
        ulong duration = state.Kernel.GlobalCycle - t0;

        DeviceChannelStats stats = state.DeviceStats[req.DeviceId][req.Channel];
        stats.TotalRequests++;
        stats.BusyCycles += duration;

        Debug.WriteLineIf(DebugEnabled, $"[MonitoringService] IORequestCompleted: Device={req.DeviceId}, Channel={req.Channel}, Pid={req.Pid}, Cycle={state.Kernel.GlobalCycle}, Duration={duration} cycles");
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _cts.Cancel();
                _cts.Dispose();
                _timer.Dispose();
                Debug.WriteLineIf(DebugEnabled, "[MonitoringService] Disposed");
            }

            _disposed = true;
        }
    }

    private sealed record CpuSnapshot(ulong CycleCount, ulong UserCycleCount, ulong SyscallCycleCount, ulong InterruptCycleCount, ulong IdleCycleCount)
    {
        public CpuSnapshot(Cpu cpu) : this(cpu.CycleCount, cpu.UserCycleCount, cpu.SyscallCycleCount, cpu.InterruptCycleCount, cpu.IdleCycleCount) { }
    }

    private sealed record ChannelSnapshot
    {
        public ChannelSnapshot() { }

        public ChannelSnapshot(ulong totalRequests, ulong busyCycles, ulong totalCycles)
        {
            TotalRequests = totalRequests;
            BusyCycles = busyCycles;
            TotalCycles = totalCycles;
        }

        public ulong TotalRequests { get; set; }
        public ulong BusyCycles { get; set; }
        public ulong TotalCycles { get; set; }
    }

    private sealed class DeviceChannelStats
    {
        public ulong TotalRequests { get; set; }
        public ulong BusyCycles { get; set; }
    }

    private sealed record CpuTotals(ulong TotalCycles, ulong TotalUser, ulong TotalSyscall, ulong TotalInterrupt, ulong TotalIdle);

    private sealed class KernelState(Kernel kernel)
    {
        public Kernel Kernel { get; } = kernel;
        public Dictionary<uint, CpuSnapshot> PrevCpu { get; } = [];
        public Dictionary<int, ulong> PrevProcCycles { get; } = [];
        public Dictionary<uint, Dictionary<uint, ChannelSnapshot>> PrevChan { get; } = [];
        public Dictionary<uint, ConcurrentDictionary<uint, bool>> DeviceChannelBusy { get; } = [];
        public ConcurrentDictionary<uint, ConcurrentDictionary<uint, DeviceChannelStats>> DeviceStats { get; } = [];
        public ConcurrentDictionary<IoRequestNotification, ulong> IoStarts { get; } = [];
    }
}
