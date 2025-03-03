namespace ProcSim.Core.Logging;

public sealed record LogEvent(int? ProcessId, string EventType, string Message, string AdditionalData = null)
{
    public DateTime Timestamp { get; init; } = DateTime.Now;
}
