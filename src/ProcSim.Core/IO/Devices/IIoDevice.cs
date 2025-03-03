using ProcSim.Core.Enums;

namespace ProcSim.Core.IO.Devices;

public interface IIoDevice
{
    string Name { get; }
    IoDeviceType DeviceType { get; }
    int Channels { get; }

    // Evento disparado quando uma requisição de I/O é concluída, simulando a interrupção do hardware.
    event Action<IoRequest> RequestCompleted;

    // Enfileira uma requisição diretamente na fila interna do dispositivo.
    void EnqueueRequest(IoRequest request);

    Task StartProcessingAsync();
}
