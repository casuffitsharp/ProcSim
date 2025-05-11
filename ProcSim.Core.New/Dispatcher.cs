namespace ProcSim.Core.New;

public class Dispatcher
{
    public void ContextSwitch(CPU cpu, PCB next)
    {
        var prev = cpu.CurrentPCB;

        if (prev?.State == ProcessState.Running)
        {
            SaveContext(cpu, prev);
            prev.State = ProcessState.Ready;
        }

        cpu.CurrentPCB = next;

        if (next != null)
        {
            next.State = ProcessState.Running;
            LoadContext(cpu, next);
        }
    }

    public static void SaveContext(CPU cpu, PCB pcb)
    {
        if (cpu.CurrentPCB is null)
            return;

        pcb.ProgramCounter = cpu.PC;
        pcb.StackPointer = cpu.SP;

        foreach (var (name, value) in cpu.RegisterFile)
            pcb.Registers[name] = value;
    }

    public static void LoadContext(CPU cpu, PCB pcb)
    {
        cpu.PC = pcb.ProgramCounter;
        cpu.SP = pcb.StackPointer;

        foreach (string name in cpu.RegisterFile.Keys.ToList())
            cpu.RegisterFile[name] = pcb.Registers[name];
    }
}
