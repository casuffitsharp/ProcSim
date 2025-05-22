using ProcSim.Core.New.Process;

namespace ProcSim.Core.New.Scheduler;

public interface IScheduler
{
    void Admit(PCB pcb);
    PCB Preempt(CPU cpu);
}