using ProcSim.Core.Enums;
using ProcSim.Core.Logging;

namespace ProcSim.Core.Monitoring;

public sealed partial class PerformanceMonitor
{
    private readonly IStructuredLogger _logger;

    private readonly Dictionary<int, CpuCounter> _cpuCounters = [];
    private readonly Dictionary<string, IoCounter> _ioCounters = [];

    private readonly PeriodicTimer _calcTimer;
    private bool _started;

    private readonly Dictionary<int, Queue<double>> _cpuUsageSamples = [];
    private readonly Dictionary<string, Queue<double>> _ioUsageSamples = [];
    private const int SmoothingWindowSize = 5;

    public PerformanceMonitor(IStructuredLogger logger)
    {
        _logger = logger;

        _calcTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        _logger.OnLog += OnLogReceived;
    }

    public event Action<Dictionary<int, double>> OnCpuUsageUpdated;
    public event Action<Dictionary<string, double>> OnIoUsageUpdated;

    public void Start()
    {
        if (!_started)
        {
            _started = true;
            _ = RunCalculationLoopAsync();
        }
    }

    public void Stop()
    {
        _calcTimer.Dispose();
    }

    private void OnLogReceived(SimEvent simEvent)
    {
        switch (simEvent)
        {
            case ProcessStateChangeEvent cpuEvent:
                ProcessCpuEvent(cpuEvent);
                break;
            case IoDeviceStateChangeEvent ioEvent:
                ProcessIoEvent(ioEvent);
                break;
            case CpuConfigurationChangeEvent cpuConfigEvent:
            {
                if (cpuConfigEvent.NewCpuCount < cpuConfigEvent.OldCpuCount)
                {
                    _cpuCounters.Keys.Where(k => k >= cpuConfigEvent.NewCpuCount).ToList().ForEach(k => _cpuCounters.Remove(k));
                    _cpuUsageSamples.Keys.Where(k => k >= cpuConfigEvent.NewCpuCount).ToList().ForEach(k => _cpuUsageSamples.Remove(k));
                }
                else if (cpuConfigEvent.NewCpuCount > cpuConfigEvent.OldCpuCount)
                {
                    for (int i = cpuConfigEvent.OldCpuCount; i < cpuConfigEvent.NewCpuCount; i++)
                    {
                        _cpuCounters[i] = new CpuCounter { SamplingStart = DateTime.UtcNow, LastEventTime = DateTime.UtcNow };
                        _cpuUsageSamples[i] = new Queue<double>();
                    }
                }
                break;
            }
            case DeviceConfigurationChangeEvent deviceConfigEvent:
            {
                if (!deviceConfigEvent.IsAdded)
                {
                    _ioCounters[deviceConfigEvent.Device] = new IoCounter { SamplingStart = DateTime.UtcNow, LastEventTime = DateTime.UtcNow };
                    _ioUsageSamples[deviceConfigEvent.Device] = new Queue<double>();
                }
                else
                {
                    _ioCounters.Remove(deviceConfigEvent.Device);
                    _ioUsageSamples.Remove(deviceConfigEvent.Device);
                }
                break;
            }
        }
    }

    private void CalculateCpuUsage()
    {
        Dictionary<int, double> cpuUsage = [];
        DateTime now = DateTime.UtcNow;
        foreach ((int coreId, CpuCounter counter) in _cpuCounters)
        {
            if (counter.IsRunning)
            {
                double delta = (now - counter.LastEventTime).TotalSeconds;
                counter.RunningTime += delta;
                counter.LastEventTime = now;
            }

            double intervalTime = (now - counter.SamplingStart).TotalSeconds;
            double rawUsage = intervalTime > 0 ? counter.RunningTime / intervalTime * 100 : 0;

            Queue<double> samples = _cpuUsageSamples[coreId];
            samples.Enqueue(rawUsage);
            if (samples.Count > SmoothingWindowSize)
                samples.Dequeue();

            cpuUsage[coreId] = samples.Average();

            counter.SamplingStart = now;
            counter.RunningTime = 0;
        }

        OnCpuUsageUpdated?.Invoke(cpuUsage);
    }

    private void ProcessCpuEvent(ProcessStateChangeEvent simEvent)
    {
        if (!_cpuCounters.TryGetValue(simEvent.Channel, out CpuCounter counter))
        {
            counter = new CpuCounter { SamplingStart = DateTime.UtcNow, LastEventTime = DateTime.UtcNow };
            _cpuCounters[simEvent.Channel] = counter;
            _cpuUsageSamples[simEvent.Channel] = new Queue<double>();
        }

        DateTime eventTime = simEvent.Timestamp;
        double delta = (eventTime - counter.LastEventTime).TotalSeconds;

        if (counter.IsRunning)
            counter.RunningTime += delta;

        counter.IsRunning = simEvent.NewState == ProcessState.Running;
        counter.LastEventTime = eventTime;
    }

    private void ProcessIoEvent(IoDeviceStateChangeEvent simEvent)
    {
        if (!_ioCounters.TryGetValue(simEvent.Device, out IoCounter counter))
        {
            counter = new IoCounter { SamplingStart = DateTime.UtcNow, LastEventTime = DateTime.UtcNow };
            _ioCounters[simEvent.Device] = counter;
            _ioUsageSamples[simEvent.Device] = new Queue<double>();
        }

        DateTime eventTime = simEvent.Timestamp;
        if (simEvent.IsActive)
        {
            if (!counter.IsActive)
            {
                counter.IsActive = true;
                counter.LastEventTime = eventTime;
            }
        }
        else
        {
            if (counter.IsActive)
            {
                double delta = (eventTime - counter.LastEventTime).TotalSeconds;
                counter.ActiveTime += delta;
                counter.IsActive = false;
                counter.LastEventTime = eventTime;
            }
        }

        counter.TotalTime = (eventTime - counter.SamplingStart).TotalSeconds;
    }

    private void CalculateIoUsage()
    {
        Dictionary<string, double> ioUsage = [];
        DateTime now = DateTime.UtcNow;
        foreach ((string device, IoCounter counter) in _ioCounters)
        {
            if (counter.IsActive)
            {
                double delta = (now - counter.LastEventTime).TotalSeconds;
                counter.ActiveTime += delta;
                counter.LastEventTime = now;
            }

            double intervalTime = (now - counter.SamplingStart).TotalSeconds;
            double rawUsage = intervalTime > 0 ? counter.ActiveTime / intervalTime * 100 : 0;

            Queue<double> samples = _ioUsageSamples[device];
            samples.Enqueue(rawUsage);
            if (samples.Count > SmoothingWindowSize)
                samples.Dequeue();

            ioUsage[device] = samples.Average();

            counter.SamplingStart = now;
            counter.ActiveTime = 0;
        }

        OnIoUsageUpdated?.Invoke(ioUsage);
    }

    private async Task RunCalculationLoopAsync()
    {
        while (await _calcTimer.WaitForNextTickAsync())
        {
            CalculateCpuUsage();
            CalculateIoUsage();
        }
    }
}
