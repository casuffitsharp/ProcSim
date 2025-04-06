using System.Collections.Concurrent;

namespace ProcSim.Core.Logging;

public class StructuredLogger : IStructuredLogger
{
    private readonly ConcurrentQueue<SimEvent> _eventQueue = new();

    public event Action<SimEvent> OnLog;

    public void Log(SimEvent simEvent)
    {
        _eventQueue.Enqueue(simEvent);
        OnLog?.Invoke(simEvent);
    }

    public IEnumerable<SimEvent> GetAllEvents() => [.. _eventQueue];
}
