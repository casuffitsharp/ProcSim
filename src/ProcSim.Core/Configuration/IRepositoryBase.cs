namespace ProcSim.Core.Configuration;

public interface IRepositoryBase<T> where T : class
{
    string FileExtension { get; }
    string FileFilter{ get; }

    Task SaveAsync(T simulation, string filePath);
    Task<T> LoadAsync(string filePath);
}