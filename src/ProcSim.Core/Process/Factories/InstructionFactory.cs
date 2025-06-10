using ProcSim.Core.IO;
using ProcSim.Core.Syscall;

namespace ProcSim.Core.Process.Factories;

public static class InstructionFactory
{
    public static Instruction Add(int r1, int r2)
    {
        return new Instruction(
            Mnemonic: $"add r0, {r1}, {r2}",
            microOps:
            [
                MicroOpFactory.SetImmediate("R1", r1),
                MicroOpFactory.SetImmediate("R2", r2),
                MicroOpFactory.FetchBinaryOp(
                    opName: "+",
                    dest: "R0",
                    leftReg: "R1", leftVal: r1,
                    rightReg: "R2", rightVal: r2,
                    operation: (a, b) => a + b
                )
            ]
        );
    }

    public static Instruction Sub(int r1, int r2)
    {
        return new Instruction(
            Mnemonic: $"sub r0, {r1}, {r2}",
            microOps:
            [
                MicroOpFactory.SetImmediate("R1", r1),
                MicroOpFactory.SetImmediate("R2", r2),
                MicroOpFactory.FetchBinaryOp(
                    opName: "-",
                    dest: "R0",
                    leftReg: "R1", leftVal: r1,
                    rightReg: "R2", rightVal: r2,
                    operation: (a, b) => a + b
                )
            ]
        );
    }

    public static Instruction Mul(int r1, int r2)
    {
        return new Instruction(
            Mnemonic: $"mul r0, {r1}, {r2}",
            microOps:
            [
                MicroOpFactory.SetImmediate("R1", r1),
                MicroOpFactory.SetImmediate("R2", r2),
                MicroOpFactory.FetchBinaryOp(
                    opName: "*",
                    dest: "R0",
                    leftReg: "R1", leftVal: r1,
                    rightReg: "R2", rightVal: r2,
                    operation: (a, b) => a + b
                )
            ]
        );
    }

    public static Instruction Div(int r1, int r2)
    {
        return new Instruction(
            Mnemonic: $"div r0, {r1}, {r2}",
            microOps:
            [
                MicroOpFactory.SetImmediate("R1", r1),
                MicroOpFactory.SetImmediate("R2", r2),
                MicroOpFactory.FetchBinaryOp(
                    opName: "/",
                    dest: "R0",
                    leftReg: "R1", leftVal: r1,
                    rightReg: "R2", rightVal: r2,
                    operation: (a, b) => a + b
                )
            ]
        );
    }

    public static Instruction AddImmediate(string register, int imm)
    {
        return new(
            Mnemonic: $"add {register}, {register}, #{imm}",
            microOps: [MicroOpFactory.AddImmediate(register, imm)]
        );
    }

    public static Instruction MovImmediate(string register, int imm)
    {
        return new(
                Mnemonic: $"mov {register}, #{imm}",
                microOps: [MicroOpFactory.MovImmediate(register, imm)]
        );
    }

    public static Instruction Blt(string leftReg, string rightReg, uint targetPc)
    {
        return new(
                Mnemonic: $"blt {leftReg}, {rightReg}, {targetPc}",
                microOps: [MicroOpFactory.BranchLessThan(leftReg, rightReg, targetPc)]
        );
    }

    public static Instruction Syscall(SyscallType type, uint deviceId = 0, uint units = 0)
    {
        return new(
        Mnemonic: type.ToString().ToLowerInvariant(),
        microOps:
        [
            MicroOpFactory.IrqEntry(),
            MicroOpFactory.SyscallHandler(type, deviceId, units),
            MicroOpFactory.SwitchContext(),
            MicroOpFactory.IrqExit()
        ]
        );
    }

    public static Instruction Jmp(uint targetPc)
    {
        return new(
            Mnemonic: $"jmp {targetPc}",
            microOps: [MicroOpFactory.JumpAbsolute(targetPc)]
        );
    }

    public static Instruction ContextSwitch()
    {
        return new(
            Mnemonic: "context_switch",
            microOps:
            [
                MicroOpFactory.IrqEntry(),
                MicroOpFactory.SwitchContext(),
                MicroOpFactory.IrqExit()
            ]
        );
    }

    public static Instruction HandleIo(IODevice device)
    {
        return new(
            Mnemonic: "handle_io",
            microOps:
            [
                MicroOpFactory.IrqEntry(),
                MicroOpFactory.IoHandler(device),
                MicroOpFactory.IrqExit()
            ]
        );
    }

    public static Instruction Idle()
    {
        return new(
            Mnemonic: "idle",
            microOps: [MicroOpFactory.Idle()]
        );
    }
}
