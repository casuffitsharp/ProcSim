using ProcSim.Core.Process;

namespace ProcSim.Core.Scheduler;

public interface IScheduler
{
    void Admit(PCB pcb);
    PCB Preempt(CPU cpu);
}