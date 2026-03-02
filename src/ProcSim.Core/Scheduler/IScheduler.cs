using ProcSim.Core.Process;

namespace ProcSim.Core.Scheduler;

public interface IScheduler
{
    void Admit(Pcb pcb);
    Pcb Preempt(Cpu cpu);
    void Decommission(Pcb pcb);
}