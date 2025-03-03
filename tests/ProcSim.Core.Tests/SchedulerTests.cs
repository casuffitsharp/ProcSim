using Moq;
using ProcSim.Core.Enums;
using ProcSim.Core.IO;
using ProcSim.Core.Logging;
using ProcSim.Core.Models;
using ProcSim.Core.Models.Operations;
using ProcSim.Core.Runtime;
using ProcSim.Core.Scheduling;
using ProcSim.Core.Scheduling.Algorithms;

namespace ProcSim.Core.Tests;

public class SchedulerTests
{
    [Fact]
    public async Task Scheduler_FcfsProcesses_RunAndComplete()
    {
        // Arrange
        var logger = new StructuredLogger();
        var tickManager = new TickManager(logger);
        tickManager.CpuTime = 10;

        // Cria um mock para a interface IIoManager
        var mockIoManager = new Mock<IIoManager>();
        // Configura para que, ao chamar DispatchRequest, o evento ProcessBecameReady seja disparado com o processo associado
        mockIoManager
            .Setup(m => m.DispatchRequest(It.IsAny<IoRequest>()))
            .Callback<IoRequest>(req =>
            {
                mockIoManager.Raise(m => m.ProcessBecameReady += null, req.Process);
            });

        var cpuScheduler = new CpuScheduler(mockIoManager.Object, logger);
        var scheduler = new Scheduler(tickManager, cpuScheduler, mockIoManager.Object);

        var operations = new List<IOperation>
        {
            new CpuOperation(3),
            new IoOperation(2, IoDeviceType.Disk)
        };
        var process = new Process(3, "TestProcessSched", operations);
        cpuScheduler.EnqueueProcess(process);

        var fcfs = new FcfsScheduling();
        using var cts = new CancellationTokenSource(20000000);

        Process lastUpdated = null;
        scheduler.ProcessUpdated += p => lastUpdated = p;

        // Act: Executa o Scheduler
            await scheduler.RunAsync(fcfs, cts.Token);
        //await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        //{
        //});

        // Assert:
        Assert.Equal(ProcessState.Completed, process.State);
        mockIoManager.Verify(m => m.DispatchRequest(It.IsAny<IoRequest>()), Times.AtLeastOnce);
    }
}
