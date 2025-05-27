using ProcSim.Core.Old.Models;

namespace ProcSim.Core.Old.Configuration;

public class ProcessesConfigRepository : RepositoryBase<List<Process>>
{
    public override string FileExtension => ".pspconfig";
    public override string FileFilter => $"Process Config Files (*{FileExtension})|*{FileExtension}";
}
