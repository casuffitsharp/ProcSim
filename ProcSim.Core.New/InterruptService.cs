namespace ProcSim.Core.New;

/// <summary>
/// Serviço de interrupções: cria micro-ops de IRQ entry/exit e context switch.
/// </summary>
public class InterruptService(Scheduler sched, Dispatcher disp)
{
    private readonly Scheduler _sched = sched;
    private readonly Dispatcher _disp = disp;

    public Queue<MicroOp> BuildISR(uint vector, CPU cpu)
    {
        Queue<MicroOp> seq = new();
        seq.Enqueue(new MicroOp("IRQ_ENTRY", c => c.TrapToKernel()));
        seq.Enqueue(new MicroOp("SWITCH_CONTEXT", c => _disp.ContextSwitch(c, _sched.PickNext(c.Id))));
        seq.Enqueue(new MicroOp("IRQ_EXIT", c => c.Iret()));
        return seq;
    }
}
