using ProcSim.Core.Process;

namespace ProcSim.Core.Interruptions.Handlers;

public class RescheduleInterruptHandler(uint vector/*, IScheduler sched*/) : IInterruptHandler
{
    private readonly uint _vector = vector;

    public bool CanHandle(uint vector)
    {
        return vector == _vector;
    }

    public Instruction BuildBody(uint vector)
    {
        //seq.Enqueue(new MicroOp("IPI_RESCHED", c =>
        //{
        //    var next = sched.Preempt(c);
        //    Dispatcher.ContextSwitch(c, next);
        //}));
        return null;
    }
}
