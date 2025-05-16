namespace ProcSim.Core.New.Interruptions.Handlers;

public interface IInterruptHandler
{
    bool CanHandle(uint vector);
    void BuildBody(uint vector, CPU cpu, Queue<MicroOp> seq);
}
