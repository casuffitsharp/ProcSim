using ProcSim.Core.New.Interruptions;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ProcSim.Core.New.IO;

public class IODevice
{
    private readonly BlockingCollection<IORequest> _queue = [];
    private readonly InterruptController _interruptController;
    private readonly ConcurrentQueue<PCB> _waiters = [];

    public IODevice(uint deviceId, uint vector, uint baseLatency, InterruptController intc)
    {
        Id = deviceId;
        Vector = vector;
        BaseLatency = baseLatency;
        _interruptController = intc;

        Task.Run(Worker);
    }

    private void Worker()
    {
        foreach (IORequest req in _queue.GetConsumingEnumerable())
        {
            Debug.WriteLine($"IODevice {Id} - Processing request from process {req.Pcb.ProcessId} (Operation Units: {req.OperationUnits})");
            Thread.Sleep((int)(req.OperationUnits * BaseLatency));
            Debug.WriteLine($"IODevice {Id} - Finished processing request from process {req.Pcb.ProcessId}");
            _waiters.Enqueue(req.Pcb);
            _interruptController.RaiseExternal(Vector);
        }
    }

    public uint Id { get; }
    public uint Vector { get; }
    public uint BaseLatency { get; }

    public void Submit(PCB pcb, uint operationUnits)
    {
        Debug.WriteLine($"IODevice {Id} - Received request from process {pcb.ProcessId} (Operation Units: {operationUnits})");
        pcb.State = ProcessState.Waiting;
        _queue.Add(new IORequest(pcb, operationUnits));
    }

    public IEnumerable<PCB> PopWaiters()
    {
        if (!_waiters.TryDequeue(out PCB pcb))
            yield break;

        yield return pcb;
    }
}
