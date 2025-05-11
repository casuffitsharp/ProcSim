using System.Collections.Concurrent;
using System.Diagnostics;

namespace ProcSim.Core.New;

/// <summary>
/// CPU virtual de um core SMP: Tick executa um micro-op ou instrução e atende IRQ.
/// </summary>
public class CPU
{
    private readonly ConcurrentDictionary<PCB, List<Instruction>> _programs;
    private readonly InterruptController _intc;
    private readonly InterruptService _isrv;
    private List<Instruction> _instructions;
    private Queue<MicroOp> _ops;

    public CPU(uint id, InterruptController intc, InterruptService isrv, Scheduler sched, Dispatcher disp, SystemCallDispatcher syscallDisp, ConcurrentDictionary<PCB, List<Instruction>> processPrograms, Action<Action> subscribeToTick)
    {
        Id = id;
        _intc = intc;
        _isrv = isrv;
        Scheduler = sched;
        Dispatcher = disp;
        subscribeToTick(Tick);

        _programs = processPrograms;
        SyscallDispatcher = syscallDisp;
    }

    public uint Id { get; }
    public PCB CurrentPCB { get; set; }
    public uint PC { get; set; }
    public uint SP { get; set; }
    public ConcurrentDictionary<string, uint> RegisterFile { get; } = new();
    public SystemCallDispatcher SyscallDispatcher { get; set; }
    public Scheduler Scheduler { get; }
    public Dispatcher Dispatcher { get; }
    public ulong CycleCount { get; private set; }
    public ulong InstructionsExecuted { get; private set; }

    private void Tick()
    {
        if (_ops?.Count > 0)
        {
            MicroOp op = _ops.Dequeue();
            Debug.WriteLine($"Process {CurrentPCB.ProcessId} - Executing micro-op: {op.Name} (PC: {PC}, SP: {SP})");
            op.Execute(this);
            CycleCount++;

            if (_ops.Count == 0)
            {
                PC++;
                CurrentPCB.ProgramCounter = PC;
                InstructionsExecuted++;
            }

            return;
        }

        if (_intc.FetchReady() is uint vec)
        {
            Debug.WriteLine($"Process {CurrentPCB.ProcessId} - Carregando ISRV");
            _ops = _isrv.BuildISR(vec, this);
            return;
        }

        if (_instructions is null || _instructions.Count == 0)
            LoadNext();

        if (_instructions?.Count > 0)
        {
            Instruction instr = _instructions.ElementAt((int)PC);
            _ops = new(instr.MicroOps);
        }
    }

    private void LoadNext()
    {
        _instructions = _programs.TryGetValue(CurrentPCB, out List<Instruction> q) ? q : null;
    }

    public void TrapToKernel() { /* hardware trap: estado mínimo salvo via micro-op */ }
    public void Iret() { /* hardware iret: contexto restaurado via micro-op */ }
}
