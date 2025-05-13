namespace ProcSim.Core.New;

public class InterruptService(Scheduler scheduler, Dispatcher dispatcher)
{
    public Queue<MicroOp> BuildISR(uint vector, CPU cpu)
    {
        return new([
            new("IRQ_ENTRY", c => c.TrapToKernel()),
            new("SWITCH_CONTEXT", c =>
            {
                PCB next = scheduler.Preempt(c);
                dispatcher.ContextSwitch(c, next);
            }),
            new("IRQ_EXIT", c => c.Iret())
        ]);
    }
}
