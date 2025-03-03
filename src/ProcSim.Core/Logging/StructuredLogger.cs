using System.Collections.Concurrent;

namespace ProcSim.Core.Logging;

public sealed class StructuredLogger : ILogger
{
    private readonly ConcurrentQueue<LogEvent> _events = new();

    public void Log(LogEvent logEvent)
    {
        _events.Enqueue(logEvent);
    }

    // Método auxiliar para registrar mensagens simples, usando um tipo de evento padrão.
    public void Log(string message)
    {
        Log(new LogEvent(null, "Info", message));
    }

    // Permite a recuperação dos eventos para visualização gráfica ou persistência.
    public IEnumerable<LogEvent> GetLogEvents()
    {
        return [.. _events];
    }
}
