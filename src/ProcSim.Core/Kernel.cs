using ProcSim.Core.Interruptions;
using ProcSim.Core.Interruptions.Handlers;
using ProcSim.Core.IO;
using ProcSim.Core.Monitoring;
using ProcSim.Core.Process;
using ProcSim.Core.Process.Factories;
using ProcSim.Core.Scheduler;
using ProcSim.Core.Syscall;
using System.Collections.Concurrent;

namespace ProcSim.Core;

public class Kernel : IDisposable
{
    private readonly Dictionary<uint, PCB> _idlePcbs = [];
    private readonly InterruptController _interruptController = new();

    private IScheduler _scheduler;

    public MonitoringService Monitoring { get; private set; }
    public Dictionary<uint, IODevice> Devices { get; } = [];
    public Dictionary<uint, CPU> Cpus { get; } = [];
    public ConcurrentDictionary<PCB, List<Instruction>> Programs { get; } = new();
    public ulong GlobalCycle { get; private set; }

    public uint RegisterDevice(string name, uint baseLatency, uint channels)
    {
        uint deviceId = (uint)Devices.Count;
        uint vec = 33 + deviceId;
        Devices.Add(deviceId, new IODevice(deviceId, vec, baseLatency, _interruptController, name, channels));
        return deviceId;
    }

    public void Initialize(uint cores, uint quantum, SchedulerType schedulerType, Action<Action> subscribeToTick)
    {
        _scheduler = SchedulerFactory.Create(schedulerType, this, _idlePcbs);

        List<IInterruptHandler> interruptHandlers =
        [
            new TimerInterruptHandler(),
            new IoInterruptHandler(Devices)
        ];
        InterruptService interruptService = new(interruptHandlers);
        SystemCallDispatcher syscallDispatcher = new(Devices);

        for (uint coreId = 0; coreId < cores; coreId++)
        {
            List<Instruction> idleInstructions = [InstructionFactory.Idle()];
            PCB idleProcess = new((int)coreId, $"Idle({coreId})", null, ProcessStaticPriority.Normal) { State = ProcessState.Ready };
            _idlePcbs[coreId] = idleProcess;
            Programs[idleProcess] = idleInstructions;

            CPU cpu = new(coreId, _interruptController, interruptService, _scheduler, syscallDispatcher, Programs, subscribeToTick);
            Cpus.Add(coreId, cpu);

            _interruptController.RegisterCore(coreId);
            _ = new TimerDevice(coreId, 32, quantum, _interruptController, subscribeToTick);
            Dispatcher.SwitchContext(cpu, idleProcess);
        }

        subscribeToTick(() => GlobalCycle++);
        //interruptController.ConfigureRedirection(32, [.. Cpus.Keys]);
    }

    public int CreateProcess(ProcessDto program)
    {
        int id = Programs.Count;
        PCB pcb = new(id, program.Name, program.Registers, program.Priority);
        Programs[pcb] = [.. program.Instructions];
        _scheduler.Admit(pcb);
        return id;
    }

    public void Dispose()
    {
        foreach (IODevice device in Devices.Values)
            device.Dispose();
    }
}
