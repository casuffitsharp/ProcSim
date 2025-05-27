using ProcSim.Core.IO;
using ProcSim.Core.Process;
using System.Diagnostics;

namespace ProcSim.Core.Syscall;

public class SystemCallDispatcher(IReadOnlyDictionary<uint, IODevice> devices)
{
    public IReadOnlyDictionary<uint, IODevice> Devices { get; } = devices;
    internal event Action<uint> ProcessTerminated = _ => { };

    public void HandleSyscall(CPU cpu, SyscallType type, uint deviceId, uint operationUnits = 0)
    {
        PCB current = cpu.CurrentPCB;
        switch (type)
        {
            case SyscallType.IoRequest:
                current.State = ProcessState.Waiting;
                Devices[deviceId].Submit(current, operationUnits);
                break;
            case SyscallType.Exit:
                Debug.WriteLine($"Process {current.ProcessId} - Exiting");
                current.State = ProcessState.Terminated;
                ProcessTerminated.Invoke(current.ProcessId);
                break;
        }
    }
}
