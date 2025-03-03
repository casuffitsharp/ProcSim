using Moq;
using ProcSim.Core.Logging;
using ProcSim.Core.Models;
using ProcSim.Core.IO;
using ProcSim.Core.Models.Operations;
using ProcSim.Core.Scheduling;

namespace ProcSim.Core.Tests;

public class CpuSchedulerTests
{
    [Fact]
    public void CpuScheduler_ReceivesProcessFromIoManagerEvent()
    {
        // Arrange: cria um mock da interface IIoManager.
        var mockIoManager = new Mock<IIoManager>();
        var logger = new StructuredLogger();
        var cpuScheduler = new CpuScheduler(mockIoManager.Object, logger);

        // Cria um processo de teste (usando uma operação simples de CPU para o exemplo).
        var operations = new List<IOperation>
        {
            new CpuOperation(5)
        };
        var process = new Process(1, "TestProcess", operations);

        // Act: simula a emissão do evento ProcessBecameReady.
        mockIoManager.Raise(m => m.ProcessBecameReady += null, process);

        // Assert: verifica se o processo foi enfileirado no CpuScheduler.
        bool dequeued = cpuScheduler.TryDequeueProcess(out Process dequeuedProcess);
        Assert.True(dequeued);
        Assert.Equal(process.Id, dequeuedProcess.Id);
    }
}
