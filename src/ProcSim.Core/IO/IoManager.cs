using System.Collections.Concurrent;
using ProcSim.Core.Enums;
using ProcSim.Core.IO.Devices;
using ProcSim.Core.Logging;
using ProcSim.Core.Models;

namespace ProcSim.Core.IO;

public sealed class IoManager(ILogger logger) : IIoManager
{
    private readonly ConcurrentDictionary<string, IIoDevice> _devices = new();

    // Evento disparado quando um processo conclui uma operação de I/O e está pronto para a CPU.
    public event Action<Process> ProcessBecameReady;

    public void AddDevice(IIoDevice device)
    {
        if (_devices.TryAdd(device.Name, device))
        {
            device.RequestCompleted += OnDeviceRequestCompleted;
            logger.Log(new LogEvent(null, "IoManager", $"Dispositivo {device.Name} adicionado."));
        }
    }

    public void RemoveDevice(string deviceName)
    {
        if (_devices.TryRemove(deviceName, out var device))
        {
            device.RequestCompleted -= OnDeviceRequestCompleted;
            logger.Log(new LogEvent(null, "IoManager", $"Dispositivo {deviceName} removido."));
        }
    }

    // Encaminha a requisição para o dispositivo adequado e registra o evento automaticamente.
    public void DispatchRequest(IoRequest request)
    {
        var device = _devices.Values.FirstOrDefault(d => d.DeviceType == request.DeviceType);
        if (device is not null)
        {
            device.EnqueueRequest(request);
            logger.Log(new LogEvent(request.Process.Id, "IoManager", $"Despachando requisição de I/O para dispositivo {request.DeviceType} com tempo restante {request.IoTime}."));
        }
        else
        {
            var msg = $"Nenhum dispositivo disponível para o tipo {request.DeviceType}";
            logger.Log(new LogEvent(request.Process.Id, "IoManager", msg));
            throw new InvalidOperationException(msg);
        }
    }

    // Quando um dispositivo conclui uma operação, loga automaticamente e dispara o evento.
    private void OnDeviceRequestCompleted(IoRequest request)
    {
        var process = request.Process;
        process.State = ProcessState.Ready;
        logger.Log(new LogEvent(process.Id, "IoManager", $"Processo {process.Id} concluiu I/O em {request.DeviceType} e está pronto."));
        ProcessBecameReady?.Invoke(process);
    }
}