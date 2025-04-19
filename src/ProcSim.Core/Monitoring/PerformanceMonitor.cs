using ProcSim.Core.Logging;
using ProcSim.Core.Runtime;
using System.Collections.Concurrent;

namespace ProcSim.Core.Monitoring;

public sealed class PerformanceMonitor
{
    private const int SmoothSampleCount = 5;

    private readonly ConcurrentDictionary<int, CpuTickCounter> _cpuCounters = new();
    private readonly ConcurrentDictionary<string, DeviceTickCounter> _ioCounters = new();
    private readonly ConcurrentDictionary<int, bool> _cpuWasRunning = new();
    private readonly ConcurrentDictionary<int, Queue<double>> _cpuHistory = new();
    private readonly ConcurrentDictionary<string, Queue<double>> _ioHistory = new();
    private readonly PeriodicTimer _calcTimer;

    public PerformanceMonitor(Kernel kernel, IStructuredLogger logger)
    {
        logger.OnLog += HandleLoggerEvent;
        kernel.OnCoreAccounting += CoreAccount;
        kernel.TickManager.OnTick += HandleTick;

        _calcTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        _ = RunCalculationLoopAsync();
    }

    public event Action<Dictionary<int, double>> OnCpuUsageUpdated;
    public event Action<Dictionary<string, double>> OnIoUsageUpdated;
    public event Action OnHardwareChanged;

    public void Reset()
    {
        _cpuCounters.Clear();
        _ioCounters.Clear();
        _cpuWasRunning.Clear();
        _cpuHistory.Clear();
        _ioHistory.Clear();
        OnHardwareChanged?.Invoke();
    }

    public IReadOnlyList<int> GetCpuCoreIds()
    {
        return [.. _cpuCounters.Keys];
    }

    public IReadOnlyList<string> GetIoDeviceNames()
    {
        return [.. _ioCounters.Keys];
    }

    private void HandleLoggerEvent(SimEvent simEvent)
    {
        switch (simEvent)
        {
            case CpuConfigurationChangeEvent cpuCfg:
                HandleCpuConfigurationChange(cpuCfg.OldCpuCount, cpuCfg.NewCpuCount);
                break;
            case DeviceConfigurationChangeEvent devCfg:
                HandleDeviceConfigurationChange(devCfg.Device, devCfg.IsAdded);
                break;
            case IoDeviceStateChangeEvent ioEvt:
                ProcessIoStateChange(ioEvt.Device, ioEvt.IsActive);
                break;
        }
    }

    private void ProcessIoStateChange(string device, bool isActive)
    {
        DeviceTickCounter counter = _ioCounters.GetOrAdd(device, _ => new DeviceTickCounter());
        _ioHistory.TryAdd(device, new Queue<double>());
        counter.IsBusy = isActive;
    }

    private void CoreAccount(int coreId, int? pid)
    {
        CpuTickCounter counter = _cpuCounters.GetOrAdd(coreId, _ => new CpuTickCounter());
        _cpuHistory.TryAdd(coreId, new Queue<double>());
        _cpuWasRunning[coreId] = pid.HasValue;
        if (pid.HasValue)
            counter.RunningTicks++;
    }

    private void HandleTick()
    {
        UpdateCpuCounters();
        UpdateIoCounters();
    }

    private void UpdateCpuCounters()
    {
        foreach (int coreId in _cpuCounters.Keys)
        {
            if (!_cpuCounters.TryGetValue(coreId, out CpuTickCounter counter))
                continue;

            counter.TotalTicks++;

            bool wasRunning = _cpuWasRunning.GetOrAdd(coreId, false);
            if (!wasRunning)
                counter.IdleTicks++;

            _cpuWasRunning[coreId] = false;
        }
    }

    private void UpdateIoCounters()
    {
        foreach (string deviceName in _ioCounters.Keys)
        {
            if (!_ioCounters.TryGetValue(deviceName, out DeviceTickCounter counter))
                continue;

            counter.TotalTicks++;

            if (counter.IsBusy)
                counter.ActiveTicks++;
        }
    }

    private void HandleCpuConfigurationChange(int oldCount, int newCount)
    {
        List<int> coresToRemove = _cpuCounters.Keys.Where(id => id > newCount).ToList();
        foreach (int coreId in coresToRemove)
        {
            _cpuCounters.TryRemove(coreId, out _);
            _cpuHistory.TryRemove(coreId, out _);
            _cpuWasRunning.TryRemove(coreId, out _);
        }

        OnHardwareChanged?.Invoke();
    }

    private void HandleDeviceConfigurationChange(string device, bool isAdded)
    {
        if (isAdded)
        {
            _ioCounters.TryRemove(device, out _);
            _ioHistory.TryRemove(device, out _);
        }
        else
        {
            _ioCounters.TryAdd(device, new DeviceTickCounter());
            _ioHistory.TryAdd(device, new Queue<double>());
        }

        OnHardwareChanged?.Invoke();
    }

    private void SampleAndReset()
    {
        UpdateCpuUsage();
        UpdateIoUsage();
    }

    private void UpdateCpuUsage()
    {
        Dictionary<int, double> cpuResult = [];
        foreach (KeyValuePair<int, CpuTickCounter> kv in _cpuCounters)
        {
            int coreId = kv.Key;
            CpuTickCounter counter = kv.Value;
            double usage = counter.TotalTicks > 0 ? counter.RunningTicks / (double)counter.TotalTicks * 100 : 0;

            Queue<double> history = _cpuHistory[coreId];
            EnqueueValue(history, usage);
            cpuResult[coreId] = history.Average();

            counter.Reset();
        }

        OnCpuUsageUpdated?.Invoke(cpuResult);
    }

    private void UpdateIoUsage()
    {
        Dictionary<string, double> ioResult = [];
        foreach (KeyValuePair<string, DeviceTickCounter> kv in _ioCounters)
        {
            string device = kv.Key;
            DeviceTickCounter counter = kv.Value;
            double usage = counter.TotalTicks > 0 ? counter.ActiveTicks / (double)counter.TotalTicks * 100 : 0;

            Queue<double> history = _ioHistory[device];
            EnqueueValue(history, usage);
            ioResult[device] = history.Average();

            counter.Reset();
        }

        OnIoUsageUpdated?.Invoke(ioResult);
    }

    private static void EnqueueValue(Queue<double> queue, double value)
    {
        queue.Enqueue(value);
        if (queue.Count > SmoothSampleCount)
            queue.Dequeue();
    }

    private async Task RunCalculationLoopAsync()
    {
        while (await _calcTimer.WaitForNextTickAsync())
            SampleAndReset();
    }

    private class CpuTickCounter
    {
        public long RunningTicks;
        public long IdleTicks;
        public long TotalTicks;

        public void Reset()
        {
            RunningTicks = IdleTicks = TotalTicks = 0;
        }
    }

    private class DeviceTickCounter
    {
        public long ActiveTicks;
        public long TotalTicks;
        public bool IsBusy;

        public void Reset()
        {
            ActiveTicks = TotalTicks = 0;
        }
    }
}
