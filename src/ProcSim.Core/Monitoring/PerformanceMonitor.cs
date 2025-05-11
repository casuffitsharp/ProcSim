using ProcSim.Core.Logging;
using ProcSim.Core.Runtime;
using System.Collections.Concurrent;

namespace ProcSim.Core.Monitoring;

public sealed class PerformanceMonitor
{
    private readonly ConcurrentDictionary<int, CoreTickCounter> _coreCounters = new();
    private readonly ConcurrentDictionary<int, int?> _lastPid = new();
    private readonly ConcurrentDictionary<string, DeviceTickCounter> _ioCounters = new();
    private readonly ConcurrentDictionary<string, Queue<double>> _ioHistory = new();

    private readonly PeriodicTimer _calcTimer;
    private bool _isPaused = false;

    public PerformanceMonitor(Kernel kernel, IStructuredLogger logger)
    {
        logger.OnLog += HandleLoggerEvent;
        kernel.OnCoreAccounting += CoreAccount;
        kernel.TickManager.OnTick += HandleTick;

        _calcTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        _ = RunCalculationLoopAsync();
    }

    public event Action<Dictionary<int, double>> OnCpuUsageUpdated;
    public event Action<Dictionary<int, Dictionary<int, double>>> OnProcessUsageByCoreUpdated;
    public event Action<Dictionary<string, double>> OnIoUsageUpdated;
    public event Action OnHardwareChanged;

    public IReadOnlyList<int> GetCpuCoreIds() => [.. _coreCounters.Keys];
    public IReadOnlyList<string> GetIoDeviceNames() => [.. _ioCounters.Keys];

    public void Pause()
    {
        _isPaused = true;
        _calcTimer.Period = TimeSpan.FromTicks(uint.MaxValue - 1);
    }

    public void Resume()
    {
        _isPaused = false;
        _calcTimer.Period = TimeSpan.FromSeconds(1);
    }

    public void Reset()
    {
        _coreCounters.Clear();
        _lastPid.Clear();
        _ioCounters.Clear();
        _ioHistory.Clear();
        OnHardwareChanged?.Invoke();
    }

    private void CoreAccount(int coreId, int? pid)
    {
        _lastPid[coreId] = pid;
        _coreCounters.TryAdd(coreId, new CoreTickCounter());
    }

    private void HandleTick()
    {
        foreach ((int coreId, var ctr) in _coreCounters)
        {

            ctr.TotalTicks++;

            if (_lastPid.TryGetValue(coreId, out int? pid) && pid.HasValue)
            {
                ctr.RunningTicks++;
                ctr.ProcessTicks.AddOrUpdate(pid.Value, 1, (_, prev) => prev + 1);
            }
            else
            {
                ctr.IdleTicks++;
            }

            _lastPid[coreId] = null;
        }

        foreach ((string _, DeviceTickCounter devCtr) in _ioCounters)
        {
            devCtr.TotalTicks++;
            if (devCtr.IsBusy) devCtr.ActiveTicks++;
        }
    }

    private async Task RunCalculationLoopAsync()
    {
        while (await _calcTimer.WaitForNextTickAsync())
        {
            if (!_isPaused)
                SampleAndReset();
        }
    }

    private void SampleAndReset()
    {
        var cpuUsage = new Dictionary<int, double>();
        var procUsageByCore = new Dictionary<int, Dictionary<int, double>>();

        foreach ((int coreId, CoreTickCounter ctr) in _coreCounters)
        {

            double total = ctr.TotalTicks;
            double run = ctr.RunningTicks;
            double pct = total > 0 ? run / total * 100 : 0;
            cpuUsage[coreId] = pct;

            var byProc = new Dictionary<int, double>();
            foreach ((int pid, long runTicks) in ctr.ProcessTicks)
                byProc[pid] = runTicks / total * 100;

            procUsageByCore[coreId] = byProc;

            ctr.Reset();
        }

        OnCpuUsageUpdated?.Invoke(cpuUsage);
        OnProcessUsageByCoreUpdated?.Invoke(procUsageByCore);

        var ioResult = new Dictionary<string, double>();
        foreach ((string device, DeviceTickCounter devCtr) in _ioCounters)
        {
            double pct = devCtr.TotalTicks > 0 ? devCtr.ActiveTicks / (double)devCtr.TotalTicks * 100 : 0;

            ioResult[device] = pct;
            devCtr.Reset();
        }

        OnIoUsageUpdated?.Invoke(ioResult);
    }

    private void HandleLoggerEvent(SimEvent e)
    {
        switch (e)
        {
            case CpuConfigurationChangeEvent c:
                foreach (var id in _coreCounters.Keys.Where(i => i > c.NewCpuCount).ToList())
                    _coreCounters.TryRemove(id, out _);

                OnHardwareChanged?.Invoke();
                break;

            case DeviceConfigurationChangeEvent d:
                if (d.IsAdded)
                {
                    _ioCounters.TryAdd(d.Device, new DeviceTickCounter());
                    _ioHistory.TryAdd(d.Device, new Queue<double>());
                }
                else
                {
                    _ioCounters.TryRemove(d.Device, out _);
                    _ioHistory.TryRemove(d.Device, out _);
                }
                OnHardwareChanged?.Invoke();
                break;

            case IoDeviceStateChangeEvent io:
                var ctr = _ioCounters.GetOrAdd(io.Device, _ => new DeviceTickCounter());
                ctr.IsBusy = io.IsActive;
                break;
        }
    }

    class CoreTickCounter
    {
        public long TotalTicks;
        public long RunningTicks;
        public long IdleTicks;

        public ConcurrentDictionary<int, long> ProcessTicks = new();

        public void Reset()
        {
            TotalTicks = RunningTicks = 0;
            ProcessTicks.Clear();
        }
    }

    class DeviceTickCounter
    {
        public long TotalTicks;
        public long ActiveTicks;
        public bool IsBusy;

        public void Reset() => TotalTicks = ActiveTicks = 0;
    }
}
