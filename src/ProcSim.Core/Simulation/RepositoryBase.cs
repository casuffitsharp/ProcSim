using System.Text.Json;

namespace ProcSim.Core.Simulation;

public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
{
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public abstract string FileExtension { get; }
    public abstract string FileFilter { get; }

    public async Task SaveAsync(T simulation, string filePath)
    {
        string json = JsonSerializer.Serialize(simulation, _options);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<T> LoadAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return default;

        string json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<T>(json);
    }
}