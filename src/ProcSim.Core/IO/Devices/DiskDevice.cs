using ProcSim.Core.Enums;
using ProcSim.Core.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ProcSim.Core.IO.Devices;

public sealed class DiskDevice : IIoDevice
{

    private readonly ConcurrentQueue<IoRequest> _requestQueue = [];
    private readonly Func<CancellationToken, Task> delayFunc;
    private readonly Func<CancellationToken> tokenProvider;
    private readonly IStructuredLogger logger;

    public DiskDevice(string name, int channels, Func<CancellationToken, Task> delayFunc, Func<CancellationToken> tokenProvider, IStructuredLogger logger)
    {
        this.delayFunc = delayFunc;
        this.tokenProvider = tokenProvider;
        this.logger = logger;
        Name = name;
        Channels = channels;

        StartProcessing();
    }

    public string Name { get; }
    public IoDeviceType DeviceType { get; } = IoDeviceType.Disk;
    public int Channels { get; }

    public event Action<IoRequest> RequestCompleted;

    public void EnqueueRequest(IoRequest request)
    {
        _requestQueue.Enqueue(request);
    }

    private void StartProcessing()
    {
        for (int i = 0; i < Channels; i++)
        {
            Debug.WriteLine($"Starting processing on channel {i} for device {Name}");
            int channel = i;
            Task.Run(async () =>
            {
                while (true)
                {
                    if (!_requestQueue.TryDequeue(out IoRequest request))
                    {
                        await delayFunc(tokenProvider());
                        continue;
                    }

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
