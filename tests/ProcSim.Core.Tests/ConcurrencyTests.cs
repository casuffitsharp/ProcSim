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

public class ConcurrencyTests
{
    private readonly IIoDevice _ioDeviceMock;

    public ConcurrencyTests()
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
    public async Task MultipleProcesses_ConcurrentExecution_NoDeadlock()
    {
        // Arrange
        StructuredLogger logger = new();
        TickManager tickManager = new(logger)
        {
            TickInterval = 5
        };

        IoManager ioManager = new(logger);
        ioManager.AddDevice(_ioDeviceMock);

        CpuScheduler cpuScheduler = new(ioManager, logger);
        SystemCallHandler sysCallHandler = new(ioManager);
        RoundRobinScheduling rr = new() { Quantum = 2 };
        Kernel kernel = new(tickManager, cpuScheduler, sysCallHandler, rr);

        // Cria 10 processos com operações mistas: CPU (3 ticks), I/O (2 ticks) e CPU (2 ticks)
        List<Process> processes = [];
        for (int i = 0; i < 10; i++)
        {
            List<IOperation> ops =
            [
                new CpuOperation(3),
                new IoOperation(2, IoDeviceType.Disk),
                new CpuOperation(2)
            ];
            Process proc = new(i, $"Process_{i}", ops);
            processes.Add(proc);
            kernel.RegisterProcess(proc);
        }

        using CancellationTokenSource cts = new(3000);

        // Act
        try
        {
            await kernel.RunAsync(cts.Token);
        }
        catch (OperationCanceledException) { }

        // Assert: Todos os processos devem estar concluídos.
        Assert.All(processes, p => Assert.Equal(ProcessState.Completed, p.State));
    }
}
