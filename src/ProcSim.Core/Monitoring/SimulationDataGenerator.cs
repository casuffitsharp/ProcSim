using ProcSim.Core.Enums;
using ProcSim.Core.Logging;

namespace ProcSim.Core.Monitoring;

public sealed class SimulationDataGenerator
{
    private readonly PeriodicTimer _timer;
    private readonly Random _random = new();
    private readonly IStructuredLogger _logger;
    private readonly int _numberOfCores;
    private readonly IList<string> _ioDevices;

    public SimulationDataGenerator(IStructuredLogger logger, int numberOfCores, IList<string> ioDevices)
    {
        _logger = logger;
        _numberOfCores = numberOfCores;
        _ioDevices = ioDevices;
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        _ = RunGeneratorAsync();
    }

    private async Task RunGeneratorAsync()
    {
        while (await _timer.WaitForNextTickAsync())
        {
            DateTime now = DateTime.UtcNow;

            // Exemplo de geração mais realista: simulação de bursts
            for (int i = 0; i < _numberOfCores; i++)
            {
                ProcessState state = (_random.NextDouble() > 0.3) ? ProcessState.Running : ProcessState.Blocked;
                _logger.Log(new ProcessStateChangeEvent
                {
                    Timestamp = now,
                    ProcessId = _random.Next(1, 10),
                    Channel = i,
                    Message = $"Core {i} mudou para {state}",
                    NewState = state
                });
            }

            foreach (string device in _ioDevices)
            {
                bool active = _random.NextDouble() > 0.6;
                _logger.Log(new IoDeviceStateChangeEvent
                {
                    Timestamp = now,
                    Device = device,
                    IsActive = active,
                    Message = $"{device} está {(active ? "Ativo" : "Inativo")}",
                });
            }
        }
    }

    public void Stop()
    {
        _timer.Dispose();
    }
}
