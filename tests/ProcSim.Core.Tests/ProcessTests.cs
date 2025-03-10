using Moq;
using ProcSim.Core.Enums;
using ProcSim.Core.IO;
using ProcSim.Core.Models;
using ProcSim.Core.Models.Operations;
using ProcSim.Core.SystemCalls;

namespace ProcSim.Core.Tests;

public class ProcessTests
{
    private IoRequest _lastRequest;
    private readonly ISysCallHandler _handlerMock;

    public ProcessTests()
    {
        Mock<ISysCallHandler> handlerMock = new();
        handlerMock
            .Setup(handler => handler.RequestIo(It.IsAny<Process>(), It.IsAny<int>(), It.IsAny<IoDeviceType>()))
            .Callback<Process, int, IoDeviceType>((process, remainingTime, deviceType) =>
            {
                _lastRequest = new IoRequest(process, remainingTime, deviceType, DateTime.Now);
            });
        _handlerMock = handlerMock.Object;
    }

    [Fact]
    public void Process_CompletesAfterCpuOperations()
    {
        // Arrange: processo com operação de CPU de duração 3.
        List<IOperation> operations = [new CpuOperation(3)];
        Process process = new(1, "ProcessoTeste", operations);

        // Act: executa 3 ticks.
        process.AdvanceTick(_handlerMock);
        process.AdvanceTick(_handlerMock);
        process.AdvanceTick(_handlerMock);

        // Assert: o processo deve estar concluído.
        Assert.Equal(ProcessState.Completed, process.State);
    }

    [Fact]
    public void Process_AdvancesToIoOperation()
    {
        // Arrange: processo com CPU de duração 2 seguida de I/O de duração 4.
        List<IOperation> operations =
        [
            new CpuOperation(2),
            new IoOperation(4, IoDeviceType.Disk)
        ];
        Process process = new(2, "ProcessoTeste2", operations);

        // Act: executa dois ticks para concluir a operação de CPU.
        process.AdvanceTick(_handlerMock);
        process.AdvanceTick(_handlerMock);

        // Assert: após a operação de CPU, a próxima é I/O, o que deve acionar o sys call e alterar o estado para Blocked.
        Assert.Equal(ProcessState.Blocked, process.State);
        Assert.NotNull(_lastRequest);
        Assert.Equal(IoDeviceType.Disk, _lastRequest.DeviceType);
    }
}
