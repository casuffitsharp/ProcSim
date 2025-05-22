using ProcSim.Core.New.Process;

namespace ProcSim.Core.New.Configuration;

public class ProcessesConfigRepository : RepositoryBase<List<ProcessDto>>
{
    public override string FileExtension => ".pspconfig";
    public override string FileFilter => $"Process Config Files (*{FileExtension})|*{FileExtension}";
}
