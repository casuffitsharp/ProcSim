using ProcSim.Core.Process;
using ProcSim.Core.Scheduler;

namespace ProcSim.Core.Interruptions.Handlers;

public class TimerInterruptHandler(IScheduler scheduler) : IInterruptHandler
{
    private const uint TimerVector = 32;

    public bool CanHandle(uint vector) => vector == TimerVector;
    public void BuildBody(uint vector, CPU cpu, Queue<MicroOp> seq)
    {
        seq.Enqueue(new MicroOp("SWITCH_CONTEXT", c =>
        {
            PCB next = scheduler.Preempt(c);
            Dispatcher.SwitchContext(c, next);
        }));
    }
}
