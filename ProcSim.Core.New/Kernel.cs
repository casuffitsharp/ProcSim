using ProcSim.Core.New.Interruptions;
using ProcSim.Core.New.Interruptions.Handlers;
using ProcSim.Core.New.IO;
using ProcSim.Core.New.Monitoring;
using ProcSim.Core.New.Process;
using ProcSim.Core.New.Scheduler;
using ProcSim.Core.New.Syscall;
using System.Collections.Concurrent;

namespace ProcSim.Core.New;

/// <summary>
/// Kernel SMP: inicializa interrupt, run-queue, devices, schedulers e cores.
/// </summary>
public class Kernel
{
    private readonly Dictionary<uint, PCB> _idlePcbs = [];
    private readonly InterruptController _interruptController = new();

    private IScheduler _scheduler;

    public MonitoringService Monitoring { get; private set; }
    public Dictionary<uint, IODevice> Devices { get; } = [];
    public Dictionary<uint, CPU> Cpus { get; } = [];
    public ConcurrentDictionary<PCB, List<Instruction>> Programs { get; } = new();
    public ulong GlobalCycle { get; private set; }

    public void RegisterDevice(string name, uint baseLatency, uint channels)
    {
        uint deviceId = (uint)Devices.Count;
        uint vec = 33 + deviceId;
        Devices.Add(deviceId, new IODevice(deviceId, vec, baseLatency, _interruptController, name, channels));
    }

    public void Initialize(uint cores, uint quantum, SchedulerType schedulerType, Action<Action> subscribeToTick)
    {
        _scheduler = SchedulerFactory.Create(schedulerType, _idlePcbs);

        List<IInterruptHandler> interruptHandlers =
        [
            new TimerInterruptHandler(_scheduler),
            new IoInterruptHandler(Devices, _scheduler)
        ];
        InterruptService interruptService = new(interruptHandlers);
        SystemCallDispatcher syscallDispatcher = new(Devices);

        for (uint coreId = 0; coreId < cores; coreId++)
        {
            PCB idleProcess = new(coreId, null, 30) { State = ProcessState.Ready };
            _idlePcbs[coreId] = idleProcess;
            Programs[idleProcess] = null;

            CPU cpu = new(coreId, _interruptController, interruptService, _scheduler, syscallDispatcher, Programs, subscribeToTick);
            Cpus.Add(coreId, cpu);

            _interruptController.RegisterCore(coreId);
            _ = new TimerDevice(coreId, 32, quantum, _interruptController, subscribeToTick);
            Dispatcher.SwitchContext(cpu, idleProcess);
        }

        subscribeToTick(() => GlobalCycle++);
        Monitoring = new MonitoringService([.. Cpus.Values], [.. Devices.Values], this, TimeSpan.FromSeconds(1));
        //interruptController.ConfigureRedirection(32, [.. Cpus.Keys]);
    }

    public void CreateProcess(ProcessDto program, uint priority = 0)
    {
        uint id = (uint)Programs.Count;
        PCB pcb = new(id, program.Registers, priority);
        Programs[pcb] = [.. program.Instructions];
        _scheduler.Admit(pcb);
    }
}
