namespace ProcSim.Core.Configuration;

public class ProcessesConfigRepository : RepositoryBase<List<ProcessConfigModel>>
{
    public override string FileExtension => ".pconfig";
    public override string FileFilter => $"Process Config Files (*{FileExtension})|*{FileExtension}";
}
