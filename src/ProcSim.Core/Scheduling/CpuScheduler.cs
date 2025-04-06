using ProcSim.Core.IO;
using ProcSim.Core.Logging;
using ProcSim.Core.Models;
using System.Collections.Concurrent;

namespace ProcSim.Core.Scheduling;

public sealed class CpuScheduler
{
    private readonly ConcurrentQueue<Process> _readyQueue = new();
    private readonly IStructuredLogger _logger;

    public CpuScheduler(IIoManager ioManager, IStructuredLogger logger)
    {
        ioManager.ProcessBecameReady += OnProcessBecameReady;
        _logger = logger;
    }

    private void OnProcessBecameReady(Process process)
    {
        EnqueueProcess(process);
        //_logger.Log(new LogEvent(process.Id, "CpuScheduler", $"Processo {process.Id} enfileirado após I/O."));
    }

    public void EnqueueProcess(Process process)
    {
        _readyQueue.Enqueue(process);
    }

    public bool TryDequeueProcess(out Process process)
    {
        return _readyQueue.TryDequeue(out process);
    }

    public IEnumerable<Process> GetReadyProcesses()
    {
        return [.. _readyQueue];
    }

    public void ClearQueue()
    {
        _readyQueue.Clear();
    }
}