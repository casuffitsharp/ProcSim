using ProcSim.Core.Interruptions.Handlers;
using ProcSim.Core.Process;
using System.Diagnostics;

namespace ProcSim.Core.Interruptions;

public class InterruptService(List<IInterruptHandler> handlers)
{
    private long _interruptCount;

    public long InterruptCount => _interruptCount;

    public Instruction BuildISR(uint vector, CPU cpu)
    {
        Interlocked.Increment(ref _interruptCount);
        IInterruptHandler handler = handlers.First(h => h.CanHandle(vector));
        Debug.WriteLine($"Building ISR for vector {vector} using {handler.GetType().Name}");
        return handler.BuildBody(vector, cpu);
    }
}
