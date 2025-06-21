using ProcSim.Core.IO;
using ProcSim.Core.Process;
using ProcSim.Core.Process.Factories;

namespace ProcSim.Core.Interruptions.Handlers;

public class IoInterruptHandler(IReadOnlyDictionary<uint, IODevice> devices) : IInterruptHandler
{
    public bool CanHandle(uint vector)
    {
        return devices.ContainsKey(vector - 33);
    }

    public Instruction BuildBody(uint vector, CPU cpu)
    {
        IODevice device = devices[vector - 33];
        return InstructionFactory.HandleIo(device);
    }
}
