namespace ProcSim.Core.New;

public class SystemCallDispatcher(IReadOnlyDictionary<uint, IODevice> devices)
{
    public IReadOnlyDictionary<uint, IODevice> Devices { get; } = devices;

    public void Dispatch(CPU cpu, SyscallType type, uint deviceId = 0)
    {
        cpu.TrapToKernel();

        var current = cpu.CurrentPCB;
        switch (type)
        {
            case SyscallType.IoRequest:
                current.State = ProcessState.Waiting;
                Devices[deviceId].Submit(current, operationUnits: 1);
                break;

            case SyscallType.Exit:
                current.State = ProcessState.Terminated;
                break;
        }

        PCB next = cpu.Scheduler.PickNext(cpu.Id);
        cpu.Dispatcher.ContextSwitch(cpu, next);

        cpu.Iret();
    }
}
