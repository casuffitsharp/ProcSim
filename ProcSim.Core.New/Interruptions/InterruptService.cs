using ProcSim.Core.New.Interruptions.Handlers;
using System.Diagnostics;

namespace ProcSim.Core.New.Interruptions;

public class InterruptService(List<IInterruptHandler> handlers)
{
    public Queue<MicroOp> BuildISR(uint vector, CPU cpu)
    {
        Queue<MicroOp> seq = new();

        seq.Enqueue(new MicroOp("IRQ_ENTRY", c => c.TrapToKernel()));

        IInterruptHandler handler = handlers.First(h => h.CanHandle(vector));
        handler.BuildBody(vector, cpu, seq);
        Debug.WriteLine($"Building ISR for vector {vector} using {handler.GetType().Name}");
        seq.Enqueue(new MicroOp("IRQ_EXIT", c => c.Iret()));

        return seq;
    }
}
