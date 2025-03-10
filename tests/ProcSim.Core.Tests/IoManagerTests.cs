using Moq;
using ProcSim.Core.Enums;
using ProcSim.Core.IO;
using ProcSim.Core.IO.Devices;
using ProcSim.Core.Logging;
using ProcSim.Core.Models;
using ProcSim.Core.Models.Operations;

namespace ProcSim.Core.Tests;

public class IoManagerTests
{
    [Fact]
    public void IoManager_DispatchRequest_CallsDeviceEnqueue()
    {
        // Arrange: cria um logger e um mock para IIoDevice.
        StructuredLogger logger = new();
        Mock<IIoDevice> mockDevice = new();
        mockDevice.SetupGet(d => d.Name).Returns("MockDevice");
        mockDevice.SetupGet(d => d.DeviceType).Returns(IoDeviceType.Disk);
        mockDevice.SetupGet(d => d.Channels).Returns(1);
        // Configura para que o método EnqueueRequest seja chamado.
        mockDevice.Setup(d => d.EnqueueRequest(It.IsAny<IoRequest>()));

        // Cria uma instância real do IoManager e adiciona o dispositivo mockado.
        IIoManager ioManager = new IoManager(logger);
        ioManager.AddDevice(mockDevice.Object);

        // Cria um processo simples com uma operação de I/O.
        List<IOperation> operations = [new IoOperation(3, IoDeviceType.Disk)];
        Process process = new(1, "TestProcess", operations);
        IoRequest ioRequest = new(process, 3, IoDeviceType.Disk, DateTime.Now);

        // Act: chama DispatchRequest.
        ioManager.DispatchRequest(ioRequest);

        // Assert: verifica se o EnqueueRequest foi chamado uma vez com o ioRequest.
        mockDevice.Verify(d => d.EnqueueRequest(ioRequest), Times.Once);
    }
}
