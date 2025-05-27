namespace ProcSim.Core.Configuration;

public class VmConfigRepository : RepositoryBase<VmConfigModel>
{
    public override string FileExtension => ".psvmconfig";
    public override string FileFilter => $"VM Config Files (*{FileExtension})|*{FileExtension}";
}
