using ProcSim.Core.Enums;
using ProcSim.Core.Models.Operations;

namespace ProcSim.Core.Models;

public sealed class Process(int id, string name, List<IOperation> operations)
{
    public int Id { get; } = id;
    public string Name { get; } = name;
    public List<IOperation> Operations { get; } = operations;
    public int CurrentOperationIndex { get; private set; } = 0;
    public ProcessState State { get; set; } = ProcessState.Ready;

    // Executa um tick da operação atual. Se a operação for completada, avança para a próxima e atualiza o estado, se necessário.
    public void ExecuteTick()
    {
        if (State == ProcessState.Completed || CurrentOperationIndex >= Operations.Count)
            return;

        var currentOperation = Operations[CurrentOperationIndex];
        currentOperation.ExecuteTick();

        if (currentOperation.IsCompleted)
        {
            CurrentOperationIndex++;
            if (CurrentOperationIndex >= Operations.Count)
            {
                State = ProcessState.Completed;
            }
        }
    }

    public IOperation GetCurrentOperation()
    {
        return Operations.ElementAtOrDefault(CurrentOperationIndex);
    }
}
