namespace ProcSim.Core.Old.Models.Operations;

public sealed class CpuOperation(int duration) : Operation(duration), ICpuOperation
{
}
