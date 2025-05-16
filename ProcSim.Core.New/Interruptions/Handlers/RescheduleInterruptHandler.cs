namespace ProcSim.Core.New.Interruptions.Handlers;

public class RescheduleInterruptHandler(uint vector/*, Scheduler sched*/) : IInterruptHandler
{
    private readonly uint _vector = vector;

    public bool CanHandle(uint vector) => vector == _vector;
    public void BuildBody(uint vector, CPU cpu, Queue<MicroOp> seq)
    {
        //seq.Enqueue(new MicroOp("IPI_RESCHED", c =>
        //{
        //    var next = sched.Preempt(c);
        //    Dispatcher.ContextSwitch(c, next);
        //}));
    }
}
