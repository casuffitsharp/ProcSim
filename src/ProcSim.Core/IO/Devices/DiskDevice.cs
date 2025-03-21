using ProcSim.Core.Enums;
using System.Threading.Channels;

namespace ProcSim.Core.IO.Devices;

public sealed class DiskDevice(string name, int channels, Func<CancellationToken, Task> delayFunc, CancellationToken cancellationToken) : IIoDevice
{
    public string Name { get; } = name;
    public IoDeviceType DeviceType { get; } = IoDeviceType.Disk;
    public int Channels { get; } = channels;

    public event Action<IoRequest> RequestCompleted;

    private readonly Channel<IoRequest> _requestChannel = Channel.CreateUnbounded<IoRequest>();

    // Enfileira a requisição de I/O diretamente na fila interna do dispositivo.
    public void EnqueueRequest(IoRequest request)
    {
        _requestChannel.Writer.TryWrite(request);
    }

    // Método que inicia o processamento da fila, simulando o tempo de I/O para cada requisição.
    public async Task StartProcessingAsync()
    {
        List<Task> tasks = [];

        for (int i = 0; i < Channels; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                await foreach (IoRequest request in _requestChannel.Reader.ReadAllAsync(cancellationToken))
                {
                    // Simula o tempo de operação de I/O.
                    for (int j = 0; j < request.IoTime && !cancellationToken.IsCancellationRequested; j++)
                        await delayFunc(cancellationToken);
                    // Notifica a conclusão da requisição (simulando a interrupção do hardware).
                    RequestCompleted?.Invoke(request);
                }
            }, cancellationToken));
        }

        await Task.WhenAll(tasks);
    }
}
