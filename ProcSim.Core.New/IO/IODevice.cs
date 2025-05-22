using ProcSim.Core.New.Interruptions;
using ProcSim.Core.New.Monitoring.Models;
using ProcSim.Core.New.Process;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ProcSim.Core.New.IO;

public class IODevice : IDisposable
{
    private readonly BlockingCollection<IORequest> _queue = [];
    private readonly InterruptController _interruptController;
    private readonly ConcurrentQueue<PCB> _waiters = [];
    private readonly List<Task> _workers;
    private bool _disposed;
    private readonly CancellationTokenSource _cts;
    
    private long _totalRequests;
    private long _totalProcessed;

    public IODevice(uint deviceId, uint vector, uint baseLatency, InterruptController intc, string name, uint channels)
    {
        Id = deviceId;
        Vector = vector;
        BaseLatency = baseLatency;
        Name = name;
        Channels = channels;
        _interruptController = intc;
        _cts = new();

        _workers = [.. Enumerable.Range(0, (int)Channels).Select(chId => Task.Run(() => WorkerAsync((uint)chId, _cts.Token)))];
    }

    public uint Id { get; }
    public uint Vector { get; }
    public uint BaseLatency { get; }
    public string Name { get; }
    public uint Channels { get; }

    public event Action<IoRequestNotification> IORequestStarted;
    public event Action<IoRequestNotification> IORequestCompleted;

    public int QueueLength => _queue.Count;
    public long TotalRequests => _totalRequests;
    public long TotalProcessed => _totalProcessed;

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

    public void Dispose()
    {
        if (_disposed)
            return;

        _queue.CompleteAdding();
        _disposed = true;
        _cts.Cancel();
        Task.WaitAll([.. _workers]);
        _queue.Dispose();
    }

    private async Task WorkerAsync(uint channelId, CancellationToken ct)
    {
        try
        {
            foreach (IORequest req in _queue.GetConsumingEnumerable(ct))
            {
                Interlocked.Increment(ref _totalRequests);
                IoRequestNotification ioRequestMonitor = new(Pid: req.Pcb.ProcessId, DeviceId: Id, Channel: channelId);
                IORequestStarted?.Invoke(ioRequestMonitor);

                Debug.WriteLine($"IODevice {Id} ({channelId}) - Processing request from process {req.Pcb.ProcessId} (Operation Units: {req.OperationUnits})");
                await Task.Delay((int)(req.OperationUnits * BaseLatency), ct);
                Debug.WriteLine($"IODevice {Id} ({channelId}) - Finished processing request from process {req.Pcb.ProcessId}");

                IORequestCompleted?.Invoke(ioRequestMonitor);
                Interlocked.Increment(ref _totalProcessed);

                _waiters.Enqueue(req.Pcb);
                _interruptController.RaiseExternal(Vector);
            }
        }
        catch (TaskCanceledException)
        {
            Debug.WriteLine($"IODevice {Id} ({channelId}) - Worker cancelled");
        }
    }
}
