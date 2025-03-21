using ProcSim.Core.Enums;
using ProcSim.Core.IO.Devices;
using ProcSim.Core.Configuration;

namespace ProcSim.Core.Tests.Configuration;

public class VmConfigRepositoryTests
{
    [Fact]
    public async Task SaveAndLoadAsync_ReturnsEquivalentVmConfig()
    {
        // Arrange
        VmConfigRepository repository = new();
        string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + repository.FileExtension);

        IoDeviceConfig device = new()
        {
            Name = "TestDevice",
            Channels = 1,
            DeviceType = IoDeviceType.Disk
        };

        VmConfig vmConfig = new()
        {
            Devices = 
            [
                new()
                {
                    Name = "Dev1",
                    Channels = 1,
                    DeviceType = IoDeviceType.Disk
                },
                new()
                {
                    Name = "Dev2",
                    Channels = 2,
                    DeviceType = IoDeviceType.USB
                },
                new()
                {
                    Name = "Dev3",
                    Channels = 4,
                    DeviceType = IoDeviceType.Memory
                }
            ],
            SchedulingAlgorithmType = SchedulingAlgorithmType.Fcfs
        };

        try
        {
            // Act
            await repository.SaveAsync(vmConfig, tempFile);
            VmConfig loadedConfig = await repository.LoadAsync(tempFile);

            // Assert
            Assert.NotNull(loadedConfig);
            Assert.Equal(vmConfig.SchedulingAlgorithmType, loadedConfig.SchedulingAlgorithmType);
            Assert.NotNull(loadedConfig.Devices);
            Assert.Equal(vmConfig.Devices.Count, loadedConfig.Devices.Count);

            for (int i = 0; i < vmConfig.Devices.Count; i++)
            {
                IoDeviceConfig expectedDevice = vmConfig.Devices[i];
                IoDeviceConfig loadedDevice = loadedConfig.Devices[i];

                Assert.Equal(expectedDevice.Name, loadedDevice.Name);
                Assert.Equal(expectedDevice.Channels, loadedDevice.Channels);
                Assert.Equal(expectedDevice.DeviceType, loadedDevice.DeviceType);
            }
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadAsync_NonExistentFile_ReturnsNull()
    {
        // Arrange
        var repository = new VmConfigRepository();
        string nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + repository.FileExtension);

        // Act
        var loadedConfig = await repository.LoadAsync(nonExistentFile);

        // Assert
        Assert.Null(loadedConfig);
    }

    [Fact]
    public void FileExtensionAndFileFilter_AreCorrect()
    {
        // Arrange
        var repository = new VmConfigRepository();

        // Act
        var extension = repository.FileExtension;
        var filter = repository.FileFilter;

        // Assert
        Assert.Equal(".psvmconfig", extension);
        Assert.Equal($"VM Config Files (*{extension})|*{extension}", filter);
    }
}
