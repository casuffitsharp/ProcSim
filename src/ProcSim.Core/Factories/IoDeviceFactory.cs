using ProcSim.Core.Enums;
using ProcSim.Core.IO.Devices;
using ProcSim.Core.Logging;

namespace ProcSim.Core.Factories;

public static class IoDeviceFactory
{
    public static IIoDevice CreateDevice(IoDeviceType deviceType, string name, int channels, Func<CancellationToken, Task> delayFunc, Func<CancellationToken> tokenProvider, IStructuredLogger logger)
    {
        IIoDevice device = deviceType switch
        {
            IoDeviceType.Disk => new DiskDevice(name, channels, delayFunc, tokenProvider, logger),
            IoDeviceType.Memory => throw new NotImplementedException(),
            IoDeviceType.USB => throw new NotImplementedException(),
            _ => throw new ArgumentException($"Tipo de dispositivo desconhecido: {deviceType}", nameof(deviceType))
        };

        device.StartProcessing();
        return device;
    }
}
