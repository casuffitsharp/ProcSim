using ProcSim.Core.Enums;
using ProcSim.Core.Logging;

namespace ProcSim.Core.Monitoring;

public sealed partial class PerformanceMonitor
{
    private readonly IStructuredLogger _logger;
    private readonly int _numberOfCores;
    private readonly IList<string> _ioDevices;

    // Dicionários para armazenar contadores atualizados via eventos do logger.
    private readonly Dictionary<int, CpuCounter> _cpuCounters = [];
    private readonly Dictionary<string, IoCounter> _ioCounters = [];

    private readonly PeriodicTimer _calcTimer;
    private bool _started;

    public event Action<Dictionary<int, double>> OnCpuUsageUpdated;
    public event Action<Dictionary<string, double>> OnIoUsageUpdated;

    private readonly Dictionary<int, Queue<double>> _cpuUsageSamples = [];
    private const int SmoothingWindowSize = 5; // número de amostras para suavização

    public PerformanceMonitor(IStructuredLogger logger, int numberOfCores, IList<string> ioDevices)
    {
        _logger = logger;
        _numberOfCores = numberOfCores;
        _ioDevices = ioDevices;

        for (int i = 0; i < _numberOfCores; i++)
        {
            _cpuCounters[i] = new CpuCounter { SamplingStart = DateTime.UtcNow, LastEventTime = DateTime.UtcNow };
            _cpuUsageSamples[i] = new Queue<double>();
        }
        foreach (string device in _ioDevices)
            _ioCounters[device] = new IoCounter { SamplingStart = DateTime.UtcNow, LastEventTime = DateTime.UtcNow };

        _calcTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        _logger.OnLog += OnLogReceived;
    }

    private void OnLogReceived(SimEvent simEvent)
    {
        if (simEvent is ProcessStateChangeEvent cpuEvent)
        {
            ProcessCpuEvent(cpuEvent);
        }
        else if (simEvent is IoDeviceStateChangeEvent ioEvent)
        {
            ProcessIoEvent(ioEvent);
        }
    }

    private void CalculateCpuUsage()
    {
        Dictionary<int, double> smoothedCpuUsage = new();
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
            double rawUsage = intervalTime > 0 ? (counter.RunningTime / intervalTime) * 100 : 0;

            // Atualize a fila de amostras para o core atual
            Queue<double> samples = _cpuUsageSamples[coreId];
            samples.Enqueue(rawUsage);
            if (samples.Count > SmoothingWindowSize)
                samples.Dequeue();

            // Calcula a média das amostras para suavizar os valores
            double smoothedValue = samples.Average();
            smoothedCpuUsage[coreId] = smoothedValue;

            // Reinicia a amostragem para o próximo período
            counter.SamplingStart = now;
            counter.RunningTime = 0;
        }
        OnCpuUsageUpdated?.Invoke(smoothedCpuUsage);
    }

    private void ProcessCpuEvent(ProcessStateChangeEvent simEvent)
    {
        if (!_cpuCounters.TryGetValue(simEvent.Channel, out CpuCounter counter))
            return;

        DateTime eventTime = simEvent.Timestamp;
        double delta = (eventTime - counter.LastEventTime).TotalSeconds;

        // Atualiza tempos baseado no estado anterior
        if (counter.IsRunning)
            counter.RunningTime += delta;

        counter.IsRunning = simEvent.NewState == ProcessState.Running;
        counter.LastEventTime = eventTime;
    }

    private void ProcessIoEvent(Core.Logging.IoDeviceStateChangeEvent simEvent)
    {
        if (!_ioCounters.TryGetValue(simEvent.Device, out IoCounter counter))
            return;

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

    public void Start()
    {
        if (!_started)
        {
            _started = true;
            _ = RunCalculationLoopAsync();
        }
    }

    private async Task RunCalculationLoopAsync()
    {
        while (await _calcTimer.WaitForNextTickAsync())
        {
            CalculateCpuUsage();
            CalculateIoUsage();
        }
    }

    private void CalculateIoUsage()
    {
        Dictionary<string, double> ioUsage = [];
        DateTime now = DateTime.Now;
        foreach ((string device, IoCounter counter) in _ioCounters)
        {
            if (counter.IsActive)
            {
                double delta = (now - counter.LastEventTime).TotalSeconds;
                counter.ActiveTime += delta;
                counter.LastEventTime = now;
            }
            counter.TotalTime = (now - counter.SamplingStart).TotalSeconds;
            double usage = counter.TotalTime > 0 ? (counter.ActiveTime / counter.TotalTime * 100) : 0;
            ioUsage[device] = usage;

            counter.SamplingStart = now;
            counter.ActiveTime = 0;
            counter.TotalTime = 0;
        }
        OnIoUsageUpdated?.Invoke(ioUsage);
    }

    public void Stop()
    {
        _calcTimer.Dispose();
    }
}
