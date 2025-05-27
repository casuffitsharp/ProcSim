using ProcSim.Core.Old.Extensions;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace ProcSim.Core.Old.Configuration;

public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
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

    public abstract string FileExtension { get; }
    public abstract string FileFilter { get; }

    public string Serialize(T data)
    {
        return JsonSerializer.Serialize(data, _options);
    }

    public async Task SaveAsync(T data, string filePath)
    {
        string json = Serialize(data);
        await File.WriteAllTextAsync(filePath, json);
    }

    public T Deserialize(string json)
    {
        return JsonSerializer.Deserialize<T>(json, _options);
    }

    public async Task<T> LoadAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return default;

        string json = await File.ReadAllTextAsync(filePath);
        return Deserialize(json);
    }
}