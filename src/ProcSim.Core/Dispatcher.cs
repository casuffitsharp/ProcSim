﻿using ProcSim.Core.Process;
using System.Diagnostics;
using System.Text.Json;

namespace ProcSim.Core;

public static class Dispatcher
{
    public static void SwitchContext(CPU cpu, PCB next)
    {
        PCB prev = cpu.CurrentPCB;
        prev?.OnExitRunning(cpu.UserCycleCount, cpu.SyscallCycleCount);
        if (prev == next)
        {
            next.OnDispatched(cpu.CycleCount, cpu.UserCycleCount, cpu.SyscallCycleCount);
            return;
        }

        if (prev != null)
            SaveContext(cpu, prev);

        cpu.CurrentPCB = next;

        if (next != null)
        {
            next.OnDispatched(cpu.CycleCount, cpu.UserCycleCount, cpu.SyscallCycleCount);
            LoadContext(cpu, next);
            next.State = ProcessState.Running;
        }
    }

    public static void SaveContext(CPU cpu, PCB pcb)
    {
        Debug.WriteLine($"Process {pcb.ProcessId} - Saving context (PC: {cpu.PC}, SP: {cpu.SP})");

        pcb.ProgramCounter = cpu.PC;
        pcb.StackPointer = cpu.SP;

        foreach ((string k, int v) in cpu.RegisterFile)
            pcb.Registers[k] = v;

        Debug.WriteLine($"Process {pcb.ProcessId} - Saved PCB: {JsonSerializer.Serialize(pcb)}");
    }

    public static void LoadContext(CPU cpu, PCB pcb)
    {
        cpu.PC = pcb.ProgramCounter;
        cpu.SP = pcb.StackPointer;

        Debug.WriteLine($"Process {pcb.ProcessId} - Loading context (PC: {cpu.PC}, SP: {cpu.SP})");

        foreach (string k in cpu.RegisterFile.Keys.ToList())
            cpu.RegisterFile[k] = pcb.Registers[k];

        Debug.WriteLine($"Process {pcb.ProcessId} - Loaded PCB: {JsonSerializer.Serialize(pcb)}");
    }
}
