using ProcSim.Core.Extensions;
using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace ProcSim.Core.Logging;

public class StructuredLogger : IStructuredLogger, IDisposable
{
    private readonly JsonSerializerOptions _options = new()
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { JsonExtensions.InheritJsonIgnore } },
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly ConcurrentBag<SimEvent> _events = [];
    public event Action<SimEvent> OnLog;

    public void Log(SimEvent simEvent)
    {
        _events.Add(simEvent);
        OnLog?.Invoke(simEvent);
    }

    public IEnumerable<SimEvent> GetAllEvents() => [.. _events];

    public void Dispose()
    {
        FlushEventsToFile();
        GC.SuppressFinalize(this);
    }

    private void FlushEventsToFile()
    {
        string directoryPath = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(directoryPath);

        string filePath = Path.Combine(directoryPath, $"SimEvents_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");

        var events = GetAllEvents();

        string json = JsonSerializer.Serialize(events, _options);
        File.WriteAllText(filePath, json);
    }
}
