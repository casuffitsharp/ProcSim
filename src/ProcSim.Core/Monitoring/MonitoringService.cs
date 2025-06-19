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
    private readonly Dictionary<uint, Dictionary<uint, ChannelSnapshot>> _prevChan = [];
    private readonly Dictionary<uint, ConcurrentDictionary<uint, bool>> _deviceChannelBusy = [];

    private readonly ConcurrentDictionary<uint, ConcurrentDictionary<uint, DeviceChannelStats>> _deviceStats = [];
    private readonly ConcurrentDictionary<IoRequestNotification, ulong> _ioStarts = [];

    private Kernel _kernel;

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
        _kernel = kernel;
        OnReset?.Invoke();
        if (_kernel is null)
        {
            Debug.WriteLineIf(DebugEnabled, "[MonitoringService] Kernel set to null.");
            return;
        }

        _prevCpu.Clear();
        _prevProcCycles.Clear();
        _prevChan.Clear();
        _deviceChannelBusy.Clear();
        _deviceStats.Clear();
        _ioStarts.Clear();
        CpuCoreMetrics.Clear();
        CpuTotalMetrics.Clear();
        ProcessMetrics.Clear();
        DeviceMetrics.Clear();

        // inicializa snapshots de CPU e processos
        foreach (CPU cpu in _kernel.Cpus.Values)
            _prevCpu[cpu.Id] = new CpuSnapshot(cpu);

        _prevGlobalCycle = kernel.GlobalCycle;

        foreach (PCB pcb in kernel.Programs.Keys)
            _prevProcCycles[pcb.ProcessId] = pcb.CpuCycles;

        foreach (IODevice device in kernel.Devices.Values)
        {
            ConcurrentDictionary<uint, DeviceChannelStats> channelStats = [];
            Dictionary<uint, ChannelSnapshot> channelSnapshots = [];
            ConcurrentDictionary<uint, bool> channelBusy = [];
            _deviceStats[device.Id] = channelStats;
            _prevChan[device.Id] = channelSnapshots;
            _deviceChannelBusy[device.Id] = channelBusy;

            for (uint channel = 0; channel < device.Channels; channel++)
            {
                channelStats[channel] = new DeviceChannelStats();
                channelSnapshots[channel] = new ChannelSnapshot();
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

                ulong dCyclesTotal = cpu.CycleCount - prev.CycleCount;
                ulong dUserCycles = cpu.UserCycleCount - prev.UserCycleCount;
                ulong dSyscallCycles = cpu.SyscallCycleCount - prev.SyscallCycleCount;
                ulong dInterruptCycles = cpu.InterruptCycleCount - prev.InterruptCycleCount;
                ulong dIdleCycles = cpu.IdleCycleCount - prev.IdleCycleCount;

                _prevCpu[cpu.Id] = new CpuSnapshot(cpu);

                totalC += dCyclesTotal;
                totalU += dUserCycles;
                totalS += dSyscallCycles;
                totalI += dInterruptCycles;
                totalIdle += dIdleCycles;

                CpuCoreMetrics.AddOrUpdate(cpu.Id, _ => [new(ts, dCyclesTotal, dUserCycles, dSyscallCycles, dInterruptCycles, dIdleCycles)], (_, lst) => { lst.Add(new(ts, dCyclesTotal, dUserCycles, dSyscallCycles, dInterruptCycles, dIdleCycles)); return lst; });
            }

            CpuTotalMetrics.Add(new(ts, totalC, totalU, totalS, totalI, totalIdle));

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

                ulong currentCycles = pcb.UserCycles + pcb.SyscallCycles;

                if (!_prevProcCycles.TryGetValue(pcb.ProcessId, out ulong prev))
                {
                    _prevProcCycles[pcb.ProcessId] = currentCycles;
                    continue;
                }

                ulong deltaProc = (currentCycles > prev) ? (currentCycles - prev) : 0UL;
                _prevProcCycles[pcb.ProcessId] = currentCycles;

                ushort procUsage = 0;
                if (deltaProc > 0 && totalC > 0UL)
                    procUsage = (ushort)Math.Min(Math.Round(100.0 * deltaProc / totalC), 100.0);

                processSnapshots.Add(new ProcessSnapshot(pcb.ProcessId, pcb.Name, pcb.State, procUsage, pcb.StaticPriority, pcb.DynamicPriority));
                ProcessUsageMetric processMetric = new()
                {
                    CpuTime = currentCycles,
                    DynamicPriority = pcb.DynamicPriority,
                    StaticPriority = pcb.StaticPriority,
                    Timestamp = ts,
                    IoTime = pcb.WaitCycles
                };
                ProcessMetrics.AddOrUpdate(pcb.ProcessId, _ => [processMetric], (_, lst) => { lst.Add(processMetric); return lst; });
            }

            ProcessListUpdated?.Invoke(processSnapshots);

            foreach ((uint deviceId, ConcurrentDictionary<uint, DeviceChannelStats> channelsStats) in _deviceStats)
            {
                _prevChan.TryGetValue(deviceId, out Dictionary<uint, ChannelSnapshot> prevChannelsSnapshots);

                DeviceUsageMetric deviceMetric = new()
                {
                    Timestamp = ts,
                    ChannelsMetrics = []
                };

                foreach ((uint channel, DeviceChannelStats channelStats) in channelsStats)
                {
                    ulong totalRequests = channelStats.TotalRequests;
                    ulong busyCycles = channelStats.BusyCycles;
                    ChannelSnapshot snapshot = new(totalRequests, busyCycles, nowTick);

                    prevChannelsSnapshots.TryGetValue(channel, out ChannelSnapshot prevSnapshot);
                    prevChannelsSnapshots[channel] = snapshot;

                    ulong dRequests = totalRequests - prevSnapshot.TotalRequests;
                    ulong dCycles = nowTick - prevSnapshot.TotalCycles;
                    ulong dBusyCycles = Math.Min(busyCycles - prevSnapshot.BusyCycles, dCycles);
                    if (dBusyCycles == 0)
                    {
                        bool isBusy = _deviceChannelBusy[deviceId].TryGetValue(channel, out bool busy) && busy;
                        if (isBusy)
                            dBusyCycles = dCycles;
                    }

                    IoChannelUsageMetric channelMetric = new(dRequests, dBusyCycles, dCycles);
                    deviceMetric.ChannelsMetrics[channel] = channelMetric;

                    deviceMetric.RequestsDelta += dRequests;
                    deviceMetric.BusyDelta += dBusyCycles;
                    deviceMetric.CyclesDelta += dCycles;
                }

                DeviceMetrics.AddOrUpdate(deviceId, _ => [deviceMetric], (_, lst) => { lst.Add(deviceMetric); return lst; });
            }

            OnMetricsUpdated?.Invoke();
        }
    }

    private void SubscribeDevice(IODevice device)
    {
        device.IORequestStarted += (req) =>
        {
            _ioStarts[req] = _kernel.GlobalCycle;
            _deviceChannelBusy[req.DeviceId][req.Channel] = true;
            Debug.WriteLineIf(DebugEnabled, $"[MonitoringService] IORequestStarted: Device={req.DeviceId}, Channel={req.Channel}, Pid={req.Pid}, Cycle={_kernel.GlobalCycle}");
        };

        device.IORequestCompleted += OnIoRequestCompleted;
    }

    private void OnIoRequestCompleted(IoRequestNotification req)
    {
        if (!_ioStarts.TryRemove(req, out ulong t0))
        {
            Debug.WriteLineIf(DebugEnabled, $"[MonitoringService] IORequestCompleted: Device={req.DeviceId}, Channel={req.Channel}, Pid={req.Pid} - Start not found!");
            return;
        }

        _deviceChannelBusy[req.DeviceId][req.Channel] = false;
        ulong duration = _kernel.GlobalCycle - t0;
        AddProcessIoMetric(req.DeviceId, req.Channel, req.Pid, duration);

        DeviceChannelStats stats = _deviceStats[req.DeviceId][req.Channel];
        stats.TotalRequests++;
        stats.BusyCycles += duration;

        Debug.WriteLineIf(DebugEnabled, $"[MonitoringService] IORequestCompleted: Device={req.DeviceId}, Channel={req.Channel}, Pid={req.Pid}, Cycle={_kernel.GlobalCycle}, Duration={duration} cycles");
    }

    private void AddProcessIoMetric(uint deviceId, uint channelId, int processId, ulong latencyCycles)
    {
        //ProcessIoMetric metric = new(
        //    Timestamp: DateTime.UtcNow,
        //    DeviceId: deviceId,
        //    ChannelId: channelId,
        //    ProcessId: processId,
        //    LatencyCycles: latencyCycles
        //);

        //(uint deviceId, uint channelId, int processId) key = (deviceId, channelId, processId);
        //ProcessIoMetrics.AddOrUpdate(key, _ => [metric], (_, lst) => { lst.Add(metric); return lst; });

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

    private record ChannelSnapshot
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

    private class DeviceChannelStats
    {
        public ulong TotalRequests { get; set; }
        public ulong BusyCycles { get; set; }
    }
}
