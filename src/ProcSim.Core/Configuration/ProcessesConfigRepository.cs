using ProcSim.Core.Models;

namespace ProcSim.Core.Configuration;

public class ProcessesConfigRepository : RepositoryBase<List<Process>>
{
    public override string FileExtension => ".pspconfig";
    public override string FileFilter => $"Process Config Files (*{FileExtension})|*{FileExtension}";
}
