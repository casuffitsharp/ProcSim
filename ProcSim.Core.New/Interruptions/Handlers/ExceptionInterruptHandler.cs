namespace ProcSim.Core.New.Interruptions.Handlers;

public class ExceptionInterruptHandler : IInterruptHandler
{
    private const uint ExceptionMax = 31;

    public bool CanHandle(uint vector) => vector <= ExceptionMax;
    public void BuildBody(uint vector, CPU cpu, Queue<MicroOp> seq)
    {
        //seq.Enqueue(new MicroOp("EXC_HANDLER", c => ExceptionHandler.Handle(c, vector)));
    }
}
