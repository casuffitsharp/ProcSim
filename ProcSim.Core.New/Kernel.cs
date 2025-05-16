using ProcSim.Core.New.Interruptions;
using ProcSim.Core.New.Interruptions.Handlers;
using ProcSim.Core.New.IO;
using System.Collections.Concurrent;

namespace ProcSim.Core.New;

/// <summary>
/// Kernel SMP: inicializa interrupt, run-queue, devices, schedulers e cores.
/// </summary>
public class Kernel
{
    private readonly Scheduler _scheduler;
    private readonly Dictionary<uint, PCB> _idlePcbs = [];

    public Dictionary<uint, IODevice> Devices { get; } = [];
    public Dictionary<uint, CPU> Cpus { get; } = [];
    public ConcurrentQueue<PCB> ReadyQueue { get; } = new();
    public ConcurrentDictionary<PCB, List<Instruction>> Programs { get; } = new();

    public Kernel()
    {
        _scheduler = new(ReadyQueue, _idlePcbs);
    }

    public void Initialize(uint cores, uint quantum, uint baseLatency, Action<Action> subscribeToTick)
    {
        InterruptController interruptController = new();

        for (uint deviceId = 0; deviceId < 4; deviceId++)
        {
            uint vec = 33 + deviceId;
            //uint core = deviceId % cores;
            //interruptController.ConfigureRedirection(vec, core);
            Devices.Add(deviceId, new IODevice(deviceId, vec, baseLatency, interruptController));
        }

        List<IInterruptHandler> interruptHandlers =
        [
            new TimerInterruptHandler(_scheduler),
            new IoInterruptHandler(Devices, _scheduler)
        ];
        InterruptService interruptService = new(interruptHandlers);
        SystemCallDispatcher syscallDispatcher = new(Devices);

        for (uint coreId = 0; coreId < cores; coreId++)
        {
            PCB idleProcess = new(coreId, null) { State = ProcessState.Ready };
            _idlePcbs[coreId] = idleProcess;
            Programs[idleProcess] = null;

            CPU cpu = new(coreId, interruptController, interruptService, _scheduler, syscallDispatcher, Programs, subscribeToTick);
            Cpus.Add(coreId, cpu);

            interruptController.RegisterCore(coreId);
            _ = new TimerDevice(coreId, 32, quantum, interruptController, subscribeToTick);
            Dispatcher.SwitchContext(cpu, idleProcess);
        }

        //interruptController.ConfigureRedirection(32, [.. Cpus.Keys]);
    }

    public void CreateProcess(Process program)
    {
        uint id = (uint)Programs.Count;
        PCB pcb = new(id, program.Registers);
        Programs[pcb] = [.. program.Instructions];
        _scheduler.Admit(pcb);
    }
}
