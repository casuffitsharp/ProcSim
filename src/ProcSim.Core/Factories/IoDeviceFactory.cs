using ProcSim.Core.Enums;
using ProcSim.Core.IO.Devices;

namespace ProcSim.Core.Factories;

public static class IoDeviceFactory
{
    public static async Task<IIoDevice> CreateDeviceAsync(IoDeviceType deviceType, string name, int channels, Func<CancellationToken, Task> delayFunc, CancellationToken cancellationToken)
    {
        IIoDevice device = deviceType switch
        {
            IoDeviceType.Disk => new DiskDevice(name, channels, delayFunc, cancellationToken),
            // Exemplo para outros tipos:
            // IODeviceType.Memory => new MemoryDevice(name, channels, delayFunc, cancellationToken),
            // IODeviceType.USB => new UsbDevice(name, channels, delayFunc, cancellationToken),
            _ => throw new ArgumentException($"Tipo de dispositivo desconhecido: {deviceType}", nameof(deviceType))
        };

        await device.StartProcessingAsync();
        return device;
    }
}
