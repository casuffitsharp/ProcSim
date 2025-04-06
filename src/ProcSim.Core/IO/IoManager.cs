using ProcSim.Core.Enums;
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
            //logger.Log(new LogEvent(null, "IoManager", $"Dispositivo {device.Name} adicionado."));
        }
    }

    public void RemoveDevice(string deviceName)
    {
        if (_devices.TryRemove(deviceName, out IIoDevice device))
        {
            device.RequestCompleted -= OnDeviceRequestCompleted;
            //logger.Log(new LogEvent(null, "IoManager", $"Dispositivo {deviceName} removido."));
        }
    }

    // Encaminha a requisição para o dispositivo adequado e registra o evento automaticamente.
    public void DispatchRequest(IoRequest request)
    {
        IIoDevice device = _devices.Values.FirstOrDefault(d => d.DeviceType == request.Operation.DeviceType);
        if (device is null)
        {
            string msg = $"Nenhum dispositivo disponível para o tipo {request.Operation.DeviceType}";
            //logger.Log(new LogEvent(request.Process.Id, "IoManager", msg));
            throw new InvalidOperationException(msg);
        }

        device.EnqueueRequest(request);
        //logger.Log(new LogEvent(request.Process.Id, "IoManager", $"Despachando requisição de I/O para dispositivo {request.Operation.DeviceType} com tempo restante {request.Operation.Duration}."));
    }

    // Quando um dispositivo conclui uma operação, loga automaticamente e dispara o evento.
    private void OnDeviceRequestCompleted(IoRequest request)
    {
        Process process = request.Process;
        process.CurrentOperationIndex++;
        process.State = ProcessState.Ready;
        //logger.Log(new LogEvent(process.Id, "IoManager", $"Processo {process.Id} concluiu I/O em {request.Operation.DeviceType} e está pronto."));
        ProcessBecameReady?.Invoke(process);
    }
}