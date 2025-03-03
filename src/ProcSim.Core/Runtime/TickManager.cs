using ProcSim.Core.Logging;

namespace ProcSim.Core.Runtime;

public sealed class TickManager
{
    private const ushort DEFAULT_CPU_TIME = 1000;
    private readonly PeriodicTimer _timer = new(TimeSpan.FromMilliseconds(DEFAULT_CPU_TIME));
    private readonly Lock _waitersLock = new();
    private readonly List<TaskCompletionSource<bool>> _tickWaiters = new();

    private readonly Lock _pauseLock = new();
    private TaskCompletionSource<bool> _resumeTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public bool IsPaused { get; private set; } = true;

    private ushort _cpuTime = DEFAULT_CPU_TIME;
    public ushort CpuTime
    {
        get => _cpuTime;
        set
        {
            if (_cpuTime == value)
                return;

            _cpuTime = value;
            _timer.Period = TimeSpan.FromMilliseconds(value);
        }
    }

    public event Action TickOccurred;
    public event Action RunStateChanged;

    private readonly ILogger _logger;

    public TickManager(ILogger logger)
    {
        _logger = logger;
        _logger.Log(new LogEvent(null, "TickManager", "TickManager iniciado."));

        // Tarefa central que aguarda ticks e notifica os aguardadores.
        _ = Task.Run(async () =>
        {
            while (await _timer.WaitForNextTickAsync(CancellationToken.None))
            {
                // Se estiver pausado, espera o resume.
                while (IsPaused)
                    await _resumeTcs.Task;

                // Notifica os inscritos no tick.
                List<TaskCompletionSource<bool>> waiters;
                lock (_waitersLock)
                {
                    waiters = new List<TaskCompletionSource<bool>>(_tickWaiters);
                    _tickWaiters.Clear();
                }
                foreach (var tcs in waiters)
                    tcs.TrySetResult(true);

                TickOccurred?.Invoke();
                _logger.Log(new LogEvent(null, "TickManager", "Tick ocorrido."));
            }
        });
    }

    public void Pause()
    {
        lock (_pauseLock)
        {
            if (IsPaused)
                return;

            IsPaused = true;
            RunStateChanged?.Invoke();
            _logger.Log(new LogEvent(null, "TickManager", "TickManager pausado."));
        }
    }

    public void Resume()
    {
        lock (_pauseLock)
        {
            if (!IsPaused)
                return;

            IsPaused = false;
            _resumeTcs.TrySetResult(true);
            _resumeTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            RunStateChanged?.Invoke();
            _logger.Log(new LogEvent(null, "TickManager", "TickManager retomado."));
        }
    }

    public async Task WaitNextTickAsync(CancellationToken ct)
    {
        while (IsPaused)
            await _resumeTcs.Task.WaitAsync(ct);

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        lock (_waitersLock)
            _tickWaiters.Add(tcs);

        using (ct.Register(() => tcs.TrySetCanceled()))
            await tcs.Task;
    }
}
