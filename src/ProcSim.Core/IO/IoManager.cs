using ProcSim.Core.Enums;
using ProcSim.Core.Factories;
using ProcSim.Core.IO.Devices;
using ProcSim.Core.Logging;
using ProcSim.Core.Models;
using System.Collections.Concurrent;

namespace ProcSim.Core.IO;

public sealed class IoManager(IStructuredLogger logger) : IIoManager
{
    private readonly ConcurrentDictionary<string, IIoDevice> _devices = new();

    public event Action<Process> ProcessBecameReady;

    public void AddDevice(IIoDevice device)
    {
        if (_devices.TryAdd(device.Name, device))
        {
            device.RequestCompleted += OnDeviceRequestCompleted;
            logger.Log(new DeviceConfigurationChangeEvent
            {
                Timestamp = DateTime.UtcNow,
                Device = device.Name,
                IsAdded = false
            });
        }
    }

    public void RemoveDevice(string deviceName)
    {
        if (_devices.TryRemove(deviceName, out IIoDevice device))
        {
            device.RequestCompleted -= OnDeviceRequestCompleted;
            logger.Log(new DeviceConfigurationChangeEvent
            {
                Timestamp = DateTime.UtcNow,
                Device = deviceName,
                IsAdded = true
            });
        }
    }

    public void Configure(IEnumerable<IoDeviceConfig> configs, Func<CancellationToken, Task> delayFunc, CancellationToken token)
    {
        foreach (string name in _devices.Keys.ToList())
            RemoveDevice(name);

        foreach (IoDeviceConfig cfg in configs.Where(c => c.IsEnabled))
        {
            IIoDevice dev = IoDeviceFactory.CreateDevice(cfg.DeviceType, cfg.Name, cfg.Channels, delayFunc, () => token, logger);
            AddDevice(dev);
        }
    }

    public void DispatchRequest(IoRequest request)
    {
        IIoDevice device = _devices.Values.FirstOrDefault(d => d.DeviceType == request.Operation.DeviceType);
        if (device is null)
        {
            string msg = $"Nenhum dispositivo disponível para o tipo {request.Operation.DeviceType}";
            throw new InvalidOperationException(msg);
        }

        device.EnqueueRequest(request);
    }

    public void Reset()
    {
        foreach (string name in _devices.Keys.ToList())
            RemoveDevice(name);
    }

    private void OnDeviceRequestCompleted(IoRequest request)
    {
        Process proc = request.Process;
        proc.CurrentOperationIndex++;
        proc.State = ProcessState.Ready;
        logger.Log(new IoDeviceStateChangeEvent
        {
            Timestamp = DateTime.UtcNow,
            Device = request.Operation.DeviceName,
            IsActive = false
        });

        ProcessBecameReady?.Invoke(proc);
    }
}
