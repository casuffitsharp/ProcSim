using System.Collections.Concurrent;

namespace ProcSim.Core.New;

/// <summary>
/// Dispositivo de I/O simulado com thread dedicada, para latência realística (Bochs/QEMU style).
/// Ao concluir operação, gera IRQ sem intervenção da CPU.
/// </summary>
public class IODevice
{
    private readonly BlockingCollection<IORequest> _queue = [];
    private readonly InterruptController _intc;

    public IODevice(uint deviceId, uint vector, uint baseLatency, InterruptController intc)
    {
        Id = deviceId;
        Vector = vector;
        BaseLatency = baseLatency;
        _intc = intc;

        Task.Run(Worker);
    }

    private void Worker()
    {
        foreach (IORequest req in _queue.GetConsumingEnumerable())
        {
            Thread.Sleep((int)(req.OperationUnits * BaseLatency));
            _intc.Raise(Vector);
        }
    }

    public uint Id { get; }
    public uint Vector { get; }
    public uint BaseLatency { get; }

    public void Submit(PCB pcb, uint operationUnits)
    {
        pcb.State = ProcessState.Waiting;
        _queue.Add(new IORequest(pcb, Id, operationUnits));
    }
}
