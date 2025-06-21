using ProcSim.Core.Process;

namespace ProcSim.Core.Interruptions.Handlers;

public interface IInterruptHandler
{
    bool CanHandle(uint vector);
    Instruction BuildBody(uint vector, CPU cpu);
}
