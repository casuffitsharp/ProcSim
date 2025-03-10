using Moq;
using ProcSim.Core.IO;
using ProcSim.Core.Logging;
using ProcSim.Core.Models;
using ProcSim.Core.Models.Operations;
using ProcSim.Core.Scheduling;

namespace ProcSim.Core.Tests;

public class CpuSchedulerTests
{
    [Fact]
    public void CpuScheduler_ReceivesProcessFromIoManagerEvent()
    {
        // Arrange: cria um mock para IIoManager
        Mock<IIoManager> mockIoManager = new();
        StructuredLogger logger = new();
        CpuScheduler cpuScheduler = new(mockIoManager.Object, logger);

        // Cria um processo com uma operação simples de CPU.
        List<IOperation> operations = [new CpuOperation(5)];
        Process process = new(1, "TestProcess", operations);

        // Act: simula a emissão do evento ProcessBecameReady.
        mockIoManager.Raise(m => m.ProcessBecameReady += null, process);

        // Assert: verifica se o processo foi enfileirado no CpuScheduler.
        bool dequeued = cpuScheduler.TryDequeueProcess(out Process dequeuedProcess);
        Assert.True(dequeued);
        Assert.Equal(process.Id, dequeuedProcess.Id);
    }
}
