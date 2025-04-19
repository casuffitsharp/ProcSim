using Moq;
using ProcSim.Core.Enums;
using ProcSim.Core.IO;
using ProcSim.Core.Logging;
using ProcSim.Core.Models;
using ProcSim.Core.Models.Operations;
using ProcSim.Core.Runtime;
using ProcSim.Core.Scheduling;
using ProcSim.Core.Scheduling.Algorithms;
using ProcSim.Core.SystemCalls;

namespace ProcSim.Core.Tests;

public class KernelTests
{
    private readonly IIoManager _ioManagerMock;
    private readonly StructuredLogger _logger = new();

    public KernelTests()
    {
        Mock<IIoManager> ioManagerMock = new();
        ioManagerMock.Setup(ioManagerMock => ioManagerMock.DispatchRequest(It.IsAny<IoRequest>()))
            .Callback<IoRequest>(request =>
            {
                Task.Run(async () =>
                {
                    await Task.Delay(50); // Simula tempo de I/O.
                    request.Process.CurrentOperationIndex++;
                    request.Process.State = ProcessState.Ready;
                    ioManagerMock.Raise(ioManagerMock => ioManagerMock.ProcessBecameReady += null, request.Process);
                    _logger.Log(new LogEvent(request.Process.Id, "FakeIoManager", "I/O request completed."));
                });
            });

        _ioManagerMock = ioManagerMock.Object;
    }

    [Fact]
    public async Task Kernel_EndToEnd_Process_CompletesSuccessfully()
    {
        // Arrange
        TickManager tickManager = new(_logger)
        {
            TickInterval = 10
        };

        // Cria o SystemCallHandler real, injetando o FakeIoManager.
        SystemCallHandler sysCallHandler = new(_ioManagerMock);

        // Cria o CpuScheduler real.
        CpuScheduler cpuScheduler = new(_ioManagerMock, _logger);

        // Escolhe FCFS como algoritmo de escalonamento.
        FcfsScheduling fcfs = new();

        // Cria o kernel injetando todas as dependências.
        IKernel kernel = new Kernel(tickManager, cpuScheduler, sysCallHandler, fcfs);

        // Cria um processo com três operações: CPU (3 ticks) → I/O (2 ticks) → CPU (2 ticks)
        List<IOperation> operations =
        [
            new CpuOperation(3),
            new IoOperation(2, IoDeviceType.Disk),
            new CpuOperation(2)
        ];
        Process process = new(100, "KernelProcess", operations);
        kernel.RegisterProcess(process);

        using CancellationTokenSource cts = new(3000);

        // Act: executa o kernel; o loop é interrompido pelo cancelamento.
        try
        {
            await kernel.RunAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Esperado, pois o loop do kernel é infinito e será cancelado.
        }

        // Assert: o processo deve estar concluído.
        Assert.Equal(ProcessState.Completed, process.State);
    }
}
