using Moq;
using ProcSim.Core.Enums;
using ProcSim.Core.IO;
using ProcSim.Core.IO.Devices;
using ProcSim.Core.Logging;
using ProcSim.Core.Models;
using ProcSim.Core.Models.Operations;
using ProcSim.Core.Runtime;
using ProcSim.Core.Scheduling;
using ProcSim.Core.Scheduling.Algorithms;
using ProcSim.Core.SystemCalls;

namespace ProcSim.Core.Tests;

public class IntegrationTests
{
    private readonly IIoDevice _ioDeviceMock;

    public IntegrationTests()
    {
        Mock<IIoDevice> ioDeviceMock = new();

        // Configura as propriedades do IoDevice fake
        ioDeviceMock.SetupGet(device => device.Name).Returns("FakeIoDevice");
        ioDeviceMock.SetupGet(device => device.DeviceType).Returns(IoDeviceType.Disk);
        ioDeviceMock.SetupGet(device => device.Channels).Returns(1);

        // Configura o método EnqueueRequest para simular o atraso de I/O e disparar o evento RequestCompleted.
        ioDeviceMock.Setup(device => device.EnqueueRequest(It.IsAny<IoRequest>()))
            .Callback<IoRequest>(request =>
            {
                // Simula um atraso de 50 ms e dispara o evento RequestCompleted.
                Task.Delay(50).ContinueWith(_ =>
                {
                    ioDeviceMock.Raise(d => d.RequestCompleted += null, request);
                });
            });
        _ioDeviceMock = ioDeviceMock.Object;
    }

    [Fact]
    public async Task Kernel_EndToEnd_ProcessFlow_CompletesSuccessfully()
    {
        // Arrange
        StructuredLogger logger = new();
        TickManager tickManager = new(logger)
        {
            TickInterval = 10
        };

        // Use o IoManager real e adicione um dispositivo fake que simula a conclusão do I/O.
        IoManager ioManager = new(logger);
        ioManager.AddDevice(_ioDeviceMock);

        // Cria o SystemCallHandler real, injetando o IoManager.
        ISysCallHandler sysCallHandler = new SystemCallHandler(ioManager);

        // Cria o CpuScheduler real.
        CpuScheduler cpuScheduler = new(ioManager, logger);

        // Escolhe um algoritmo simples (FCFS) para o escalonamento.
        FcfsScheduling fcfs = new();

        // Cria o Kernel, injetando todas as dependências.
        Kernel kernel = new(tickManager, cpuScheduler, sysCallHandler, fcfs);

        // Cria um processo com três operações: CPU (3 ticks) → I/O (2 ticks) → CPU (2 ticks)
        List<IOperation> operations =
        [
            new CpuOperation(3),
            new IoOperation(2, IoDeviceType.Disk),
            new CpuOperation(2)
        ];
        Process process = new(5, "IntegrationProcess", operations);
        kernel.RegisterProcess(process);

        using CancellationTokenSource cts = new(500);

        // Act
        try
        {
            await kernel.RunAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            // O kernel roda em loop infinito; usamos o cancelamento para interromper.
        }

        // Assert: o processo deve estar concluído.
        Assert.Equal(ProcessState.Completed, process.State);
    }
}
