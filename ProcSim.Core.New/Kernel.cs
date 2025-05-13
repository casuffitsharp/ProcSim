using System.Collections.Concurrent;

namespace ProcSim.Core.New;

/// <summary>
/// Kernel SMP: inicializa interrupt, run-queue, devices, schedulers e cores.
/// </summary>
public class Kernel
{
    private readonly InterruptController _interruptController;
    private readonly Dispatcher _dispatcher;
    private readonly Scheduler _scheduler;
    private readonly InterruptService _interruptService;
    private readonly SystemCallDispatcher _syscallDispatcher;
    private readonly Dictionary<uint, PCB> _idlePcbs = [];

    public Dictionary<uint, IODevice> Devices { get; } = [];
    public Dictionary<uint, CPU> Cpus { get; } = [];
    public ConcurrentQueue<PCB> ReadyQueue { get; } = new();
    public ConcurrentDictionary<PCB, List<Instruction>> Programs { get; } = new();

    public Kernel()
    {
        _interruptController = new();
        _dispatcher = new();
        _scheduler = new(ReadyQueue, _idlePcbs);
        _interruptService = new(_scheduler, _dispatcher);
        _syscallDispatcher = new(Devices);
    }

    public void Initialize(uint cores, uint quantum, uint baseLatency, Action<Action> subscribeToTick)
    {
        _ = new TimerDevice(vector: 32, quantum, _interruptController, subscribeToTick);

        // Dispositivos I/O (vetores 33-36)
        for (uint deviceId = 0; deviceId < 4; deviceId++)
            Devices.Add(deviceId, new IODevice(deviceId, 33 + deviceId, baseLatency, _interruptController));

        // Cores
        for (uint id = 0; id < cores; id++)
        {
            PCB idleProcess = new(id, null) { State = ProcessState.Ready };
            _idlePcbs[id] = idleProcess;
            Programs[idleProcess] = null;

            CPU cpu = new(id, _interruptController, _interruptService, _scheduler, _dispatcher, _syscallDispatcher, Programs, subscribeToTick);
            Cpus.Add(id, cpu);
            _dispatcher.ContextSwitch(cpu, idleProcess);
        }
    }

    public void CreateProcess(Process program)
    {
        uint id = (uint)Programs.Count;
        PCB pcb = new(id, program.Registers);
        Programs[pcb] = [.. program.Instructions];
        _scheduler.Admit(pcb);
    }
}
