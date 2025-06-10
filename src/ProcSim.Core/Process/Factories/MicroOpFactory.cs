using ProcSim.Core.IO;
using ProcSim.Core.Syscall;

namespace ProcSim.Core.Process.Factories;

public static class MicroOpFactory
{
    public static MicroOp SetImmediate(string register, int value)
    {
        return new(
            Name: $"SET_{register}_{value}",
            Description: $"{register} ← {value}",
            Execute: cpu => cpu.RegisterFile[register] = value
        );
    }

    public static MicroOp FetchBinaryOp(string opName, string dest, string leftReg, int leftVal, string rightReg, int rightVal, Func<int, int, int> operation)
    {
        return new(
            Name: $"{opName}_{dest}_{leftReg}_{rightReg}",
            Description: $"{dest} ← {leftReg}({leftVal}) {opName} {rightReg}({rightVal})",
            Execute: cpu =>
            {
                int a = cpu.RegisterFile[leftReg];
                int b = cpu.RegisterFile[rightReg];
                cpu.RegisterFile[dest] = operation(a, b);
            }
        );
    }

    public static MicroOp AddImmediate(string register, int imm)
    {
        return new(
            Name: $"ADD_IMM_{register}_{imm}",
            Description: $"{register} ← {register} + {imm}",
            Execute: cpu => cpu.RegisterFile[register] += imm
        );
    }

    public static MicroOp MovImmediate(string register, int imm)
    {
        return new(
            Name: $"MOV_IMM_{register}_{imm}",
            Description: $"{register} ← {imm}",
            Execute: cpu => cpu.RegisterFile[register] = imm
        );
    }

    public static MicroOp BranchLessThan(string leftReg, string rightReg, uint targetPc)
    {
        return new(
            Name: "BLT",
            Description: $"Se {leftReg} < {rightReg} então PC ← {targetPc}",
            Execute: cpu =>
            {
                if (cpu.RegisterFile[leftReg] < cpu.RegisterFile[rightReg])
                    cpu.PC = targetPc;
            }
        );
    }

    public static MicroOp IrqEntry()
    {
        return new(
            Name: "IRQ_ENTRY",
            Description: "Salvar contexto e entrar em kernel",
            Execute: cpu => cpu.TrapToKernel()
        );
    }

    public static MicroOp SwitchContext()
    {
        return new(
            Name: "SWITCH_CONTEXT",
            Description: "Troca de contexto para próximo processo",
            Execute: cpu =>
            {
                PCB next = cpu.Scheduler.Preempt(cpu);
                Dispatcher.SwitchContext(cpu, next);
            }
        );
    }

    public static MicroOp IrqExit()
    {
        return new(
            Name: "IRQ_EXIT",
            Description: "Restaurar contexto e sair de kernel",
            Execute: cpu => cpu.Iret()
        );
    }

    public static MicroOp JumpAbsolute(uint targetPc)
    {
        return new(
            Name: "JMP",
            Description: $"PC ← {targetPc}",
            Execute: cpu => cpu.PC = targetPc
        );
    }

    public static MicroOp IoHandler(IODevice device)
    {
        return new(
            Name: "IO_HANDLER",
            Description: "Finaliza I/O e admite processos aguardando no dispositivo",
            Execute: cpu =>
            {
                foreach (PCB pcb in device.PopWaiters())
                {
                    pcb.OnIoCompleted(cpu.CycleCount);
                    cpu.Scheduler.Admit(pcb);
                }
            }
        );
    }

    public static MicroOp Idle()
    {
        return new(
            Name: "IDLE",
            Description: "CPU ociosa – aguardando próxima interrupção",
            Execute: cpu => cpu.PC = 0
        );
    }

    public static MicroOp SyscallHandler(SyscallType type, uint deviceId, uint units)
    {
        return new(
            Name: "SYSCALL_HANDLER",
            Description: $"Handler {type}",
            Execute: cpu => cpu.SyscallDispatcher.HandleSyscall(cpu, type, deviceId, units)
        );
    }
}
