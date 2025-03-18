﻿namespace ProcSim.Core.Simulation;

public class VmConfigRepository : RepositoryBase<VmConfig>
{
    public override string FileExtension => ".psvmconfig";
    public override string FileFilter => $"VM Config Files (*{FileExtension})|*{FileExtension}";
}
