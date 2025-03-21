using ProcSim.Core.Models;
using ProcSim.Core.Models.Operations;
using ProcSim.Core.Enums;
using ProcSim.Core.Configuration;

namespace ProcSim.Core.Tests.Configuration;

public class ProcessesConfigRepositoryTests
{
    [Fact]
    public async Task SaveAndLoadAsync_ReturnsEquivalentProcessList_WithNestedOperations()
    {
        // Arrange
        ProcessesConfigRepository repository = new();
        string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + repository.FileExtension);

        List<IOperation> operations =
        [
            new CpuOperation(1),
            new IoOperation(2, IoDeviceType.Memory),
            new CpuOperation(1),
            new IoOperation(2, IoDeviceType.Disk),
            new CpuOperation(1),
            new IoOperation(2, IoDeviceType.USB)
        ];

        Process process1 = new(1, "P1", operations);
        Process process2 = new(2, "P2", operations);

        List<Process> processes =
        [
            process1,
            process2
        ];

        try
        {
            // Act
            await repository.SaveAsync(processes, tempFile);
            List<Process> loadedProcesses = await repository.LoadAsync(tempFile);

            // Assert
            Assert.NotNull(loadedProcesses);
            Assert.Equal(processes.Count, loadedProcesses.Count);

            for (int i = 0; i < processes.Count; i++)
            {
                var expectedProcess = processes[i];
                var loadedProcess = loadedProcesses[i];

                Assert.Equal(expectedProcess.Id, loadedProcess.Id);
                Assert.Equal(expectedProcess.Name, loadedProcess.Name);
                Assert.Equal(expectedProcess.Operations.Count, loadedProcess.Operations.Count);

                for (int j = 0; j < expectedProcess.Operations.Count; j++)
                {
                    var expectedOp = expectedProcess.Operations[j];
                    var loadedOp = loadedProcess.Operations[j];

                    Assert.Equal(expectedOp.Duration, loadedOp.Duration);

                    switch (expectedOp)
                    {
                        case CpuOperation:
                            Assert.IsType<CpuOperation>(loadedOp);
                            break;
                        case IoOperation expectedIo:
                            var loadedIo = Assert.IsType<IoOperation>(loadedOp);
                            Assert.Equal(expectedIo.DeviceType, loadedIo.DeviceType);
                            break;
                        default:
                            Assert.Fail("Unexpected operation type");
                            break;
                    }
                }
            }
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadAsync_NonExistentFile_ReturnsNull_ForProcessesConfigRepository()
    {
        // Arrange
        var repository = new ProcessesConfigRepository();
        string nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + repository.FileExtension);

        // Act
        var loadedProcesses = await repository.LoadAsync(nonExistentFile);

        // Assert
        Assert.Null(loadedProcesses);
    }

    [Fact]
    public void FileExtensionAndFileFilter_AreCorrect_ForProcessesConfigRepository()
    {
        // Arrange
        var repository = new ProcessesConfigRepository();

        // Act
        var extension = repository.FileExtension;
        var filter = repository.FileFilter;

        // Assert
        Assert.Equal(".pspconfig", extension);
        Assert.Equal($"Process Config Files (*{extension})|*{extension}", filter);
    }
}
