namespace ProcSim.Core
{
    public class TickManager
    {
        private const ushort DEFAULT_CPU_TIME = 1000;
        private readonly PeriodicTimer _timer = new(TimeSpan.FromMilliseconds(DEFAULT_CPU_TIME));
        private readonly Lock _waitersLock = new();
        private readonly List<TaskCompletionSource<bool>> _tickWaiters = [];

        private readonly Lock _pauseLock = new();
        private TaskCompletionSource<bool> _resumeTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public bool IsPaused { get; private set; } = true;

        public ushort CpuTime
        {
            get => field;
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

        public TickManager()
        {
            // Inicia a tarefa central que aguarda ticks e notifica os aguardadores.
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
                        waiters = [.. _tickWaiters];
                        _tickWaiters.Clear();
                    }
                    foreach (var tcs in waiters)
                        tcs.TrySetResult(true);

                    TickOccurred?.Invoke();
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

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            lock (_waitersLock)
                _tickWaiters.Add(tcs);

            using (ct.Register(() => tcs.TrySetCanceled()))
                await tcs.Task;
        }
    }
}
