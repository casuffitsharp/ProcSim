using Moq;
using ProcSim.Core.IO;
using ProcSim.Core.Logging;
using ProcSim.Core.Models;
using ProcSim.Core.Models.Operations;
using ProcSim.Core.Enums;
using ProcSim.Core.IO.Devices;

namespace ProcSim.Core.Tests;

public class IoManagerTests
{
    [Fact]
    public void IoManager_DispatchRequest_CallsDeviceEnqueue()
    {
        // Arrange: Cria um logger fake.
        var logger = new StructuredLogger();

        // Cria um mock para IIODevice.
        var mockDevice = new Mock<IIoDevice>();
        mockDevice.SetupGet(d => d.Name).Returns("MockDevice");
        mockDevice.SetupGet(d => d.DeviceType).Returns(IoDeviceType.Disk);
        mockDevice.SetupGet(d => d.Channels).Returns(1);
        // Configura para que o método EnqueueRequest seja chamado.
        mockDevice.Setup(d => d.EnqueueRequest(It.IsAny<IoRequest>()));

        // Cria uma instância real do IoManager usando a interface.
        IIoManager ioManager = new IoManager(logger);
        ioManager.AddDevice(mockDevice.Object);

        // Cria um processo simples com uma operação de I/O.
        List<IOperation> operations = new System.Collections.Generic.List<IOperation>
        {
            new IoOperation(3, IoDeviceType.Disk)
        };
        var process = new Process(1, "TestProcess", operations);
        var ioRequest = new IoRequest(process, 3, IoDeviceType.Disk, DateTime.Now);

        // Act: chama o DispatchRequest.
        ioManager.DispatchRequest(ioRequest);

        // Assert: verifica se o EnqueueRequest foi chamado uma vez com o ioRequest.
        mockDevice.Verify(d => d.EnqueueRequest(ioRequest), Times.Once);
    }
}
