using ProcSim.Core.IO;
using ProcSim.Core.Process;
using ProcSim.Core.Scheduler;

namespace ProcSim.Core.Interruptions.Handlers;

public class IoInterruptHandler(IReadOnlyDictionary<uint, IODevice> devices, IScheduler scheduler) : IInterruptHandler
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
