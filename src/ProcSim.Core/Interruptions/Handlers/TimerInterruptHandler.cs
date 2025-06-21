using ProcSim.Core.Process;
using ProcSim.Core.Process.Factories;

namespace ProcSim.Core.Interruptions.Handlers;

public class TimerInterruptHandler() : IInterruptHandler
{
    private const uint TimerVector = 32;

    public bool CanHandle(uint vector)
    {
        return vector == TimerVector;
    }

    public Instruction BuildBody(uint vector, CPU cpu)
    {
        return InstructionFactory.ContextSwitch();
    }
}
