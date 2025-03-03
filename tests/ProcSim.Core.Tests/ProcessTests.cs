using ProcSim.Core.Enums;
using ProcSim.Core.Models;
using ProcSim.Core.Models.Operations;

namespace ProcSim.Core.Tests;

public class ProcessTests
{
    [Fact]
    public void Process_CompletesAfterAllTicks()
    {
        // Arrange: cria um processo com uma operação de CPU de duração 3.
        var operations = new List<IOperation>
        {
            new CpuOperation(3)
        };
        var process = new Process(1, "TestProcess", operations);

        // Act: executa 3 ticks.
        process.ExecuteTick();
        process.ExecuteTick();
        process.ExecuteTick();

        // Assert: o processo deve estar concluído.
        Assert.Equal(ProcessState.Completed, process.State);
    }

    [Fact]
    public void Process_AdvancesToNextOperation()
    {
        // Arrange: cria um processo com duas operações de CPU.
        var operations = new List<IOperation>
        {
            new CpuOperation(2),
            new CpuOperation(2)
        };
        var process = new Process(2, "TestProcess2", operations);

        // Act & Assert:
        // Primeiro tick: operação 1 (2 -> 1).
        process.ExecuteTick();
        Assert.NotEqual(ProcessState.Completed, process.State);
        Assert.Equal(1, operations[0].RemainingTime);

        // Segundo tick: operação 1 completa, passa para operação 2.
        process.ExecuteTick();
        Assert.NotEqual(ProcessState.Completed, process.State);
        Assert.Equal(2, operations[1].RemainingTime);

        // Terceiro tick: operação 2 (2 -> 1).
        process.ExecuteTick();
        Assert.NotEqual(ProcessState.Completed, process.State);
        Assert.Equal(1, operations[1].RemainingTime);

        // Quarto tick: operação 2 completa, processo concluído.
        process.ExecuteTick();
        Assert.Equal(ProcessState.Completed, process.State);
    }
}
