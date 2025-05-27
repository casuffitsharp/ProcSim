using ProcSim.Core.Interruptions.Handlers;
using ProcSim.Core.Process;
using System.Diagnostics;

namespace ProcSim.Core.Interruptions;

public class InterruptService(List<IInterruptHandler> handlers)
{
    private long _interruptCount;

    public long InterruptCount => _interruptCount;

    public Queue<MicroOp> BuildISR(uint vector, CPU cpu)
    {
        Interlocked.Increment(ref _interruptCount);

        Queue<MicroOp> seq = new();

        seq.Enqueue(new MicroOp("IRQ_ENTRY", c => c.TrapToKernel()));

        IInterruptHandler handler = handlers.First(h => h.CanHandle(vector));
        handler.BuildBody(vector, cpu, seq);
        Debug.WriteLine($"Building ISR for vector {vector} using {handler.GetType().Name}");
        seq.Enqueue(new MicroOp("IRQ_EXIT", c => c.Iret()));

        return seq;
    }
}
