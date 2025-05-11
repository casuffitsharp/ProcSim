namespace ProcSim.Core.New;

/// <summary>
/// Fábrica de instruções de syscall que produzem micro-ops de entrada e saída.
/// </summary>
public static class SyscallFactory
{
    public static Instruction Create(SyscallType type, uint deviceId = 0)
    {
        return new(type.ToString(),
        [
            new MicroOp("SYSCALL_ENTRY", cpu => cpu.TrapToKernel()),
            new MicroOp("SYSCALL_DISPATCH", cpu => cpu.SyscallDispatcher.Dispatch(cpu, type, deviceId)),
            new MicroOp("SYSCALL_EXIT", cpu => cpu.Iret())
        ]);
    }
}
