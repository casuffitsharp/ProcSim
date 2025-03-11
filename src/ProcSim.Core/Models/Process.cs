using ProcSim.Core.Enums;
using ProcSim.Core.Models.Operations;
using ProcSim.Core.SystemCalls;

namespace ProcSim.Core.Models;

public sealed class Process(int id, string name, List<IOperation> operations)
{
    public int Id { get; } = id;
    public string Name { get; } = name;
    public List<IOperation> Operations { get; } = operations;
    public int CurrentOperationIndex { get; internal set; } = 0;
    public ProcessState State { get; set; } = ProcessState.Ready;

    public IOperation GetCurrentOperation()
    {
        return Operations.ElementAtOrDefault(CurrentOperationIndex);
    }

    public void AdvanceTick(ISysCallHandler sysCallHandler)
    {
        IOperation currentOp = GetCurrentOperation();
        if (currentOp is IIoOperation operation)
        {
            // Se j� estiver em I/O, o processo j� deve estar bloqueado.
            sysCallHandler.RequestIo(this, currentOp.RemainingTime, operation.DeviceType);
            State = ProcessState.Blocked;
            return;
        }

        if (currentOp is ICpuOperation)
        {
            currentOp.ExecuteTick();
            if (!currentOp.IsCompleted)
                return;

            // CPU burst conclu�da; avan�a para a pr�xima opera��o.
            CurrentOperationIndex++;
            if (CurrentOperationIndex >= Operations.Count)
            {
                State = ProcessState.Completed;
                return;
            }

            AdvanceTick(sysCallHandler);
            return;
        }
    }

    public void Reset()
    {
        CurrentOperationIndex = 0;
        State = ProcessState.Ready;
        foreach (IOperation operation in Operations)
            operation.Reset();
    }
}
