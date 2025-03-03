namespace ProcSim.Core.Models.Operations;

public sealed class CpuOperation(int duration) : Operation(duration), ICpuOperation
{
}
