using ProcSim.Core.Old.Enums;
using ProcSim.Core.Old.Models.Operations;
using ProcSim.Core.Old.SystemCalls;
using System.Text.Json.Serialization;

namespace ProcSim.Core.Old.Models;

public sealed class Process(int id, string name, List<IOperation> operations)
{
    private int _currentOpChannel = 0;

    public event Action StateChanged;
    public event Action OperationChanged;

    public int Id { get; } = id;
    public string Name { get; } = name;
    public List<IOperation> Operations { get; } = operations;

    [JsonIgnore]
    public int CurrentOperationIndex
    {
        get;
        internal set
        {
            if (field != value)
            {
                field = value;
                OperationChanged?.Invoke();
            }
        }
    } = 0;

    [JsonIgnore]
    public ProcessState State
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                StateChanged?.Invoke();
            }
        }
    } = ProcessState.Ready;


    public IOperation GetCurrentOperation()
    {
        return Operations.ElementAtOrDefault(CurrentOperationIndex);
    }

    public void AdvanceTick(ISysCallHandler sysCallHandler)
    {
        IOperation currentOp = GetCurrentOperation();
        //if (currentOp.Channel.HasValue)
        //    _currentOpChannel = currentOp.Channel.Value;
        //else
        //    currentOp.Channel = _currentOpChannel;

        if (currentOp is IoOperation operation)
        {
            // Se já estiver em I/O, o processo já deve estar bloqueado.
            sysCallHandler.RequestIo(this, operation);
            State = ProcessState.Blocked;
            return;
        }

        if (currentOp is ICpuOperation)
        {
            currentOp.ExecuteTick();
            if (!currentOp.IsCompleted)
                return;

            // CPU burst concluída; avança para a próxima operação.
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
