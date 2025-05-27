namespace ProcSim.Core.Old.Configuration;

public interface IRepositoryBase<T> where T : class
{
    string FileExtension { get; }
    string FileFilter { get; }

    Task SaveAsync(T data, string filePath);
    Task<T> LoadAsync(string filePath);
    string Serialize(T data);
    T Deserialize(string json);
}