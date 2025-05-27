using ProcSim.Core.IO;
using ProcSim.Core.Monitoring.Models;
using ProcSim.Core.Process;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ProcSim.Core.Monitoring;

public class MonitoringService : IDisposable
{
    private readonly List<CPU> _cpus;
    private readonly List<IODevice> _devices;
    private readonly Kernel _kernel;
    private readonly PeriodicTimer _timer;
    private readonly CancellationTokenSource _cts = new();

    private readonly Dictionary<uint, CpuSnapshot> _prevCpu = [];
    private ulong _prevGlobalCycle;
    private readonly Dictionary<uint, ulong> _prevProcCycles = [];
    private readonly Dictionary<DeviceChannelKey, ChannelSnapshot> _prevChan = [];

    private readonly ConcurrentDictionary<DeviceChannelKey, ChannelStats> _channelStats = [];
    private readonly ConcurrentDictionary<IoRequestNotification, ulong> _ioStarts = [];

    public MonitoringService(List<CPU> cpus, List<IODevice> devices, Kernel kernel, TimeSpan interval)
    {
        _cpus = cpus;
        _devices = devices;
        _kernel = kernel;
        _timer = new PeriodicTimer(interval);

        // inicializa snapshots de CPU e processos
        foreach (CPU cpu in _cpus)
            _prevCpu[cpu.Id] = new CpuSnapshot(cpu);

        _prevGlobalCycle = kernel.GlobalCycle;

        foreach (PCB pcb in kernel.Programs.Keys)
            _prevProcCycles[pcb.ProcessId] = pcb.CpuCycles;

        // configura estatísticas por canal e subscreve eventos
        foreach (IODevice device in _devices)
        {
            for (uint channel = 0; channel < device.Channels; channel++)
            {
                DeviceChannelKey key = new(device.Id, channel);
                _channelStats[key] = new ChannelStats();
                _prevChan[key] = new ChannelSnapshot(0, 0);
            }
            SubscribeDevice(device);
        }

        Debug.WriteLine($"[MonitoringService] Initialized with {cpus.Count} CPUs, {devices.Count} devices, interval={interval.TotalMilliseconds}ms");

        Task.Run(() => CollectAsync(_cts.Token));
    }

    public ConcurrentDictionary<uint, List<CpuCoreUsageMetric>> CpuCoreMetrics { get; } = [];
    public List<CpuTotalUsageMetric> CpuTotalMetrics { get; } = [];
    public ConcurrentDictionary<uint, List<ProcessCpuUsageMetric>> ProcessMetrics { get; } = [];
    public ConcurrentDictionary<DeviceChannelKey, List<IoChannelUsageMetric>> IoChannelMetrics { get; } = [];
    public ConcurrentDictionary<uint, List<DeviceAggregateUsageMetric>> DeviceAggregateMetrics { get; } = [];
    public ConcurrentDictionary<(uint DeviceId, uint ChannelId, uint ProcessId), List<ProcessIoMetric>> ProcessIoMetrics { get; } = [];

    private async Task CollectAsync(CancellationToken ct)
    {
        while (await _timer.WaitForNextTickAsync(ct))
        {
            DateTime ts = DateTime.UtcNow;
            ulong nowTick = _kernel.GlobalCycle;
            ulong intervalCycles = nowTick - _prevGlobalCycle;
            _prevGlobalCycle = nowTick;

            // 1) CPU por núcleo e total
            ulong totalC = 0;
            ulong totalU = 0;
            ulong totalS = 0;
            ulong totalI = 0;

            foreach (CPU cpu in _cpus)
            {
                CpuSnapshot prev = _prevCpu[cpu.Id];
                ulong dC = cpu.CycleCount - prev.CycleCount;
                ulong dU = cpu.UserCycleCount - prev.UserCycleCount;
                ulong dS = cpu.SyscallCycleCount - prev.SyscallCycleCount;
                ulong dI = cpu.InterruptCycleCount - prev.InterruptCycleCount;
                _prevCpu[cpu.Id] = new CpuSnapshot(cpu);

                CpuCoreMetrics.AddOrUpdate(cpu.Id, _ => [new(ts, cpu.Id, dC, dU, dS, dI)], (_, lst) => { lst.Add(new(ts, cpu.Id, dC, dU, dS, dI)); return lst; });
                Debug.WriteLine($"[MonitoringService] CPU {cpu.Id}: ΔCycles={dC}, ΔUser={dU}, ΔSyscall={dS}, ΔInterrupt={dI}");

                totalC += dC;
                totalU += dU;
                totalS += dS;
                totalI += dI;
            }

            CpuTotalMetrics.Add(new CpuTotalUsageMetric(ts, totalC, totalU, totalS, totalI));
            Debug.WriteLine($"[MonitoringService] CPU Total: ΔCycles={totalC}, ΔUser={totalU}, ΔSyscall={totalS}, ΔInterrupt={totalI}");

            // 2) CPU por processo
            foreach (PCB pcb in _kernel.Programs.Keys)
            {
                ulong prev = _prevProcCycles.GetValueOrDefault(pcb.ProcessId);
                ulong dP = pcb.CpuCycles - prev;
                _prevProcCycles[pcb.ProcessId] = pcb.CpuCycles;

                ProcessMetrics.AddOrUpdate(pcb.ProcessId, _ => [new(ts, pcb.ProcessId, dP)], (_, lst) => { lst.Add(new(ts, pcb.ProcessId, dP)); return lst; });
                Debug.WriteLine($"[MonitoringService] Process {pcb.ProcessId}: ΔCpuCycles={dP}");
            }

            // 3) I/O por canal e agregado por dispositivo
            foreach (IODevice device in _devices)
            {
                long aggReq = 0;
                double aggUtil = 0;
                uint chanCount = device.Channels;

                for (uint channel = 0; channel < chanCount; channel++)
                {
                    DeviceChannelKey key = new(device.Id, channel);
                    ChannelStats stats = _channelStats[key];
                    ChannelSnapshot prev = _prevChan[key];

                    // deltas
                    long dReq = stats.TotalRequests - prev.TotalRequests;
                    ulong dBusy = stats.BusyCycles - prev.BusyCycles;

                    // cálculo de utilização [% de busy no intervalo]
                    double util = intervalCycles > 0 ? (double)dBusy / intervalCycles : 0;

                    IoChannelMetrics.AddOrUpdate(key, _ => [new(ts, device.Id, channel, dReq, util)], (_, lst) => { lst.Add(new(ts, device.Id, channel, dReq, util)); return lst; });

                    Debug.WriteLine($"[MonitoringService] Device {device.Id} Channel {channel}: ΔRequests={dReq}, ΔBusy={dBusy}, Util={util:P2}");

                    aggReq += dReq;
                    aggUtil += util;

                    // atualiza snapshot
                    _prevChan[key] = new ChannelSnapshot(stats);
                }

                // métrica agregada (média de utilização entre canais)
                double avgUtil = chanCount > 0 ? aggUtil / chanCount : 0;
                DeviceAggregateMetrics.AddOrUpdate(device.Id, _ => [new(ts, device.Id, aggReq, avgUtil)], (_, lst) => { lst.Add(new(ts, device.Id, aggReq, avgUtil)); return lst; });
                Debug.WriteLine($"[MonitoringService] Device {device.Id} Aggregate: ΔRequests={aggReq}, AvgUtil={avgUtil:P2}");
            }
        }
    }

    private void SubscribeDevice(IODevice device)
    {
        device.IORequestStarted += (req) =>
        {
            _ioStarts[req] = _kernel.GlobalCycle;
            Debug.WriteLine($"[MonitoringService] IORequestStarted: Device={req.DeviceId}, Channel={req.Channel}, Pid={req.Pid}, Cycle={_kernel.GlobalCycle}");
        };

        device.IORequestCompleted += (req) =>
        {
            if (_ioStarts.TryRemove(req, out ulong t0))
            {
                ulong duration = _kernel.GlobalCycle - t0;
                AddProcessIoMetric(
                    req.DeviceId,
                    req.Channel,
                    req.Pid,
                    duration
                );

                // atualiza stats do canal
                DeviceChannelKey key = new(req.DeviceId, req.Channel);
                ChannelStats stats = _channelStats[key];
                stats.TotalRequests++;
                stats.BusyCycles += duration;

                Debug.WriteLine($"[MonitoringService] IORequestCompleted: Device={req.DeviceId}, Channel={req.Channel}, Pid={req.Pid}, Duration={duration} cycles");
            }
            else
            {
                Debug.WriteLine($"[MonitoringService] IORequestCompleted: Device={req.DeviceId}, Channel={req.Channel}, Pid={req.Pid} - Start not found!");
            }
        };
    }

    private void AddProcessIoMetric(uint deviceId, uint channelId, uint processId, ulong latencyCycles)
    {
        ProcessIoMetric metric = new(
            Timestamp: DateTime.UtcNow,
            DeviceId: deviceId,
            ChannelId: channelId,
            ProcessId: processId,
            LatencyCycles: latencyCycles
        );

        (uint deviceId, uint channelId, uint processId) key = (deviceId, channelId, processId);
        ProcessIoMetrics.AddOrUpdate(key, _ => [metric], (_, lst) => { lst.Add(metric); return lst; });

        Debug.WriteLine($"[MonitoringService] ProcessIoMetric: Device={deviceId}, Channel={channelId}, Process={processId}, Latency={latencyCycles} cycles");
    }

    public void Dispose()
    {
        _cts.Cancel();
        _timer.Dispose();
        Debug.WriteLine("[MonitoringService] Disposed");
    }

    private record CpuSnapshot(ulong CycleCount, ulong UserCycleCount, ulong SyscallCycleCount, ulong InterruptCycleCount)
    {
        public CpuSnapshot(CPU cpu) : this(cpu.CycleCount, cpu.UserCycleCount, cpu.SyscallCycleCount, cpu.InterruptCycleCount) { }
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
