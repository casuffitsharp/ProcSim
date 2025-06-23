namespace ProcSim.Core.Configuration;

public class VmConfigRepository : RepositoryBase<VmConfigModel>
{
    public override string FileExtension => ".vmconfig";
    public override string FileFilter => $"VM Config Files (*{FileExtension})|*{FileExtension}";
}
