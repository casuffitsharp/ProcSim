using ProcSim.Core.Logging;

namespace ProcSim.Core.Runtime;

public sealed class TickManager
{
    private const ushort DEFAULT_CPU_TIME = 1000;
    private readonly PeriodicTimer _timer = new(TimeSpan.FromMilliseconds(DEFAULT_CPU_TIME));
    private readonly Lock _waitersLock = new();
    private readonly List<TaskCompletionSource<bool>> _tickWaiters = [];

    private readonly Lock _pauseLock = new();
    private TaskCompletionSource<bool> _resumeTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly IStructuredLogger _logger;

    public TickManager(IStructuredLogger logger)
    {
        _logger = logger;
        //_logger.Log(new LogEvent(null, "TickManager", "TickManager iniciado."));

        // Tarefa central que aguarda ticks e notifica os aguardadores.
        Task.Run(async () =>
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
                    waiters = [.. _tickWaiters];
                    _tickWaiters.Clear();
                }
                foreach (TaskCompletionSource<bool> tcs in waiters)
                    tcs.TrySetResult(true);

                TickOccurred?.Invoke();
                //_logger.Log(new LogEvent(null, "TickManager", "Tick ocorrido."));
            }
        });
    }

    public bool IsPaused { get; private set; } = true;

    public ushort CpuTime
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            _timer.Period = TimeSpan.FromMilliseconds(value);
        }
    } = DEFAULT_CPU_TIME;

    public event Action TickOccurred;
    public event Action RunStateChanged;

    public void Pause()
    {
        lock (_pauseLock)
        {
            if (IsPaused)
                return;

            IsPaused = true;
            RunStateChanged?.Invoke();
            //_logger.Log(new LogEvent(null, "TickManager", "TickManager pausado."));
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
            //_logger.Log(new LogEvent(null, "TickManager", "TickManager retomado."));
        }
    }

    public async Task WaitNextTickAsync(CancellationToken ct)
    {
        while (IsPaused)
            await _resumeTcs.Task.WaitAsync(ct);

        TaskCompletionSource<bool> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

        lock (_waitersLock)
            _tickWaiters.Add(tcs);

        using (ct.Register(() => tcs.TrySetCanceled()))
            await tcs.Task;
    }

    public async Task DelayFunc(CancellationToken ct)
    {
        await WaitNextTickAsync(ct);
    }
}
