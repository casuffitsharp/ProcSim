namespace ProcSim.Core.Logging;

public class SimEvent
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; }
    public int? ProcessId { get; set; }
    public int? CoreId { get; set; }
    public string Component { get; set; }
    public string Message { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}
