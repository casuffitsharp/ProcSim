using Moq;
using ProcSim.Core.Enums;
using ProcSim.Core.IO;
using ProcSim.Core.Logging;
using ProcSim.Core.Models;
using ProcSim.Core.Models.Operations;
using ProcSim.Core.SystemCalls;

namespace ProcSim.Core.Tests;

public class SystemCallHandlerTests
{
    [Fact]
    public void RequestIo_DispatchesIoRequest_CallsIoManagerDispatch()
    {
        // Arrange
        StructuredLogger logger = new();
        Mock<IIoManager> mockIoManager = new();
        SystemCallHandler sysCallHandler = new(mockIoManager.Object);
        List<IOperation> operations = [new IoOperation(4, IoDeviceType.Disk)];
        Process process = new(10, "TestProcessSysCall", operations);

        // Act
        sysCallHandler.RequestIo(process, operations[0].RemainingTime, IoDeviceType.Disk);

        // Assert
        mockIoManager.Verify(m => m.DispatchRequest(It.Is<IoRequest>(req => req.Process.Id == process.Id && req.IoTime == 4 && req.DeviceType == IoDeviceType.Disk)), Times.Once);
    }
}
