namespace ProcSim.Core.New;

public static class SyscallFactory
{
    public static Instruction Create(SyscallType type, uint deviceId = 0)
    {
        return new(type.ToString().ToLowerInvariant(),
        [
            new MicroOp("SYSCALL_ENTRY",   cpu => cpu.TrapToKernel()),
            new MicroOp("SYSCALL_HANDLER", cpu => cpu.SyscallDispatcher.HandleSyscall(cpu, type, deviceId)),
            new MicroOp("SCHEDULE", cpu =>
            {
                PCB next = cpu.Scheduler.Preempt(cpu);
                cpu.Dispatcher.ContextSwitch(cpu, next);
            }),
            new MicroOp("SYSCALL_EXIT", cpu => cpu.Iret())
        ]);
    }
}