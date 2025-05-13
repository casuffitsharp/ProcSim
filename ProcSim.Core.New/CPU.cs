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

        for (uint i = 0; i < 8; i++)
            RegisterFile[$"R{i}"] = 0;

        RegisterFile["ZF"] = 0;
        RegisterFile["CF"] = 0;
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
    public ulong InstructionsFetched { get; private set; }

    private void Tick()
    {
        if (_ops?.Count > 0)
        {
            MicroOp op = _ops.Dequeue();
            Debug.WriteLine($"Process {CurrentPCB.ProcessId} - Executing micro-op: {op.Name} (PC: {PC}, SP: {SP})");
            op.Execute(this);
            CycleCount++;

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

        if (_instructions?.Count > 0 && PC < _instructions.Count)
        {
            Instruction instr = _instructions.ElementAt((int)PC);
            InstructionsFetched++;
            Debug.WriteLine($"Process {CurrentPCB.ProcessId} - Executing instruction: {instr.Mnemonic} (PC: {PC}, SP: {SP})");
            _ops = new(instr.MicroOps);
            PC++;
        }
    }

    private void LoadNext()
    {
        Debug.WriteLine($"Process {CurrentPCB.ProcessId} - Carregando próximo programa");
        _programs.TryGetValue(CurrentPCB, out List<Instruction> q);
        _instructions = q;
        PC = 0;
        if (_instructions is null)
        {
            Debug.WriteLine($"Process {CurrentPCB.ProcessId} - Programa não encontrado");
            return;
        }
    }

    public void TrapToKernel() { /* hardware trap: estado mínimo salvo via micro-op */ }
    public void Iret() { /* hardware iret: contexto restaurado via micro-op */ }
}
