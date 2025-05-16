using ProcSim.Core.New.IO;

namespace ProcSim.Core.New.Interruptions.Handlers;

public class IoInterruptHandler(IReadOnlyDictionary<uint, IODevice> devices, Scheduler scheduler) : IInterruptHandler
{
    public bool CanHandle(uint vector) => devices.ContainsKey(vector - 33);
    public void BuildBody(uint vector, CPU cpu, Queue<MicroOp> seq)
    {
        IODevice device = devices[vector - 33];
        seq.Enqueue(new MicroOp("IO_HANDLER", c =>
        {
            foreach (PCB pcb in device.PopWaiters())
                scheduler.Admit(pcb);
        }));
    }
}
