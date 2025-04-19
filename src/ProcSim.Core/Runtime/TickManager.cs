using ProcSim.Core.Logging;

namespace ProcSim.Core.Runtime;

public sealed class TickManager
{
    private const ushort DEFAULT_TICK_INTERVAL = 1000;
    private readonly PeriodicTimer _timer = new(TimeSpan.FromMilliseconds(DEFAULT_TICK_INTERVAL));
    private readonly Lock _waitersLock = new();
    private readonly List<TaskCompletionSource<bool>> _tickWaiters = [];

    private readonly Lock _pauseLock = new();
    private TaskCompletionSource<bool> _resumeTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly IStructuredLogger _logger;

    public TickManager(IStructuredLogger logger)
    {
        _logger = logger;

        Task.Run(async () =>
        {
            while (await _timer.WaitForNextTickAsync(CancellationToken.None))
            {
                while (IsPaused)
                    await _resumeTcs.Task;

                List<TaskCompletionSource<bool>> waiters;
                lock (_waitersLock)
                {
                    waiters = [.. _tickWaiters];
                    _tickWaiters.Clear();
                }

                foreach (TaskCompletionSource<bool> tcs in waiters)
                    tcs.TrySetResult(true);

                OnTick?.Invoke();
            }
        });
    }

    public bool IsPaused { get; private set; } = true;

    public ushort TickInterval
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            _timer.Period = TimeSpan.FromMilliseconds(value);
        }
    } = DEFAULT_TICK_INTERVAL;

    public event Action OnTick;
    public event Action RunStateChanged;

    public void Pause()
    {
        lock (_pauseLock)
        {
            if (IsPaused)
                return;

            IsPaused = true;
            RunStateChanged?.Invoke();
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
