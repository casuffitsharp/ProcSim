using ProcSim.Core.Enums;
using ProcSim.Core.Logging;
using System.Threading.Channels;

namespace ProcSim.Core.IO.Devices;

public sealed class DiskDevice(string name, int channels, Func<CancellationToken, Task> delayFunc, Func<CancellationToken> tokenProvider, IStructuredLogger logger) : IIoDevice
{
    public string Name { get; } = name;
    public IoDeviceType DeviceType { get; } = IoDeviceType.Disk;
    public int Channels { get; } = channels;

    public event Action<IoRequest> RequestCompleted;

    private readonly Channel<IoRequest> _requestChannel = Channel.CreateUnbounded<IoRequest>();

    public void EnqueueRequest(IoRequest request)
    {
        _requestChannel.Writer.TryWrite(request);
    }

    public void StartProcessing()
    {
        for (int i = 0; i < Channels; i++)
        {
            int channel = i;
            Task.Run(async () =>
            {
                while (await _requestChannel.Reader.WaitToReadAsync(tokenProvider()))
                {
                    if (!_requestChannel.Reader.TryRead(out IoRequest request))
                        continue;

                    request.Operation.DeviceName = Name;
                    request.Operation.Channel = channel;

                    logger.Log(new IoDeviceStateChangeEvent
                    {
                        Timestamp = DateTime.UtcNow,
                        Device = Name,
                        IsActive = true,
                    });

                    for (int j = 0; j < request.Operation.Duration && !tokenProvider().IsCancellationRequested; j++)
                    {
                        await delayFunc(tokenProvider());
                        request.Operation.ExecuteTick();
                    }

                    RequestCompleted?.Invoke(request);
                }
            }, tokenProvider());
        }
    }
}
