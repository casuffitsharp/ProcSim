using ProcSim.Core.Interruptions;
using ProcSim.Core.Process;
using ProcSim.Core.Scheduler;
using ProcSim.Core.Syscall;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ProcSim.Core;

public class CPU
{
    private readonly ConcurrentDictionary<PCB, List<Instruction>> _programs;
    private readonly InterruptController _interruptController;
    private readonly InterruptService _isrv;
    private List<Instruction> _instructions;
    private ConcurrentQueue<MicroOp> _ops;

    public CPU(uint id, InterruptController intc, InterruptService isrv, IScheduler sched, SystemCallDispatcher syscallDisp, ConcurrentDictionary<PCB, List<Instruction>> processPrograms, Action<Action> subscribeToTick)
    {
        Id = id;
        _interruptController = intc;
        _isrv = isrv;
        Scheduler = sched;
        subscribeToTick(Tick);

        _programs = processPrograms;
        SyscallDispatcher = syscallDisp;

        for (uint i = 0; i < 8; i++)
            RegisterFile[$"R{i}"] = 0;

        RegisterFile["ZF"] = 0;
        RegisterFile["CF"] = 0;
    }

    public uint Id { get; }
    public PCB CurrentPCB
    {
        get;
        set
        {
            field = value;
            _instructions = null;
            _ops = null;
        }
    }
    public uint PC { get; set; }
    public uint SP { get; set; }
    public ConcurrentDictionary<string, int> RegisterFile { get; } = new();
    public SystemCallDispatcher SyscallDispatcher { get; set; }
    public IScheduler Scheduler { get; }
    public ulong CycleCount { get; private set; }
    public ulong InstructionsFetched { get; private set; }
    public ulong UserCycleCount { get; private set; }
    public ulong SyscallCycleCount { get; private set; }
    public ulong InterruptCycleCount { get; private set; }
    public ulong IdleCycleCount { get; private set; }

    private void Tick()
    {
        CycleCount++;

        if (_ops?.TryDequeue(out MicroOp op) == true)
        {
            Debug.WriteLine($"Process {CurrentPCB.ProcessId} - Executing micro-op: {op.Name} (PC: {PC}, SP: {SP})");
            op.Execute(this);

            if (op.Name.StartsWith("SYSCALL"))
            {
                SyscallCycleCount++;
                CurrentPCB.SyscallCycles++;
            }
            else if (op.Name.StartsWith("IRQ_") || op.Name.Equals("SWITCH_CONTEXT"))
            {
                InterruptCycleCount++;
            }
            else if (op.Name.StartsWith("IDLE"))
            {
                IdleCycleCount++;
            }
            else
            {
                UserCycleCount++;
                CurrentPCB.UserCycles++;
            }

            return;
        }

        if (_interruptController.FetchReady(Id) is uint vector)
        {
            Debug.WriteLine($"Process {CurrentPCB.ProcessId} - Carregando ISRV");
            Instruction instruction = _isrv.BuildISR(vector);
            _ops = new(instruction.MicroOps);
            InstructionsFetched++;
            InterruptCycleCount++;
            return;
        }

        if (_instructions is null || _instructions.Count == 0)
            LoadNext();

        if (_instructions?.Count > 0 && PC < _instructions.Count)
        {
            Instruction instr = _instructions.ElementAt((int)PC);
            InstructionsFetched++;
            SyscallCycleCount++;
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
        if (_instructions is null)
        {
            Debug.WriteLine($"Process {CurrentPCB.ProcessId} - Programa não encontrado");
            return;
        }
    }

    public void TrapToKernel() { /* hardware trap: estado mínimo salvo via micro-op */ }
    public void Iret() { /* hardware iret: contexto restaurado via micro-op */ }
}
