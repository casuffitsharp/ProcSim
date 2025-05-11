using System.Collections.Concurrent;

namespace ProcSim.Core.New;

/// <summary>
/// Escalonador central, sem associação a core: permite migração de processos.
/// </summary>
public class Scheduler(ConcurrentQueue<PCB> readyQueue, Dictionary<uint, PCB> idlePcbs)
{
    private readonly ConcurrentQueue<PCB> _readyQueue = readyQueue;

    public void Admit(PCB pcb)
    {
        pcb.State = ProcessState.Ready;
        _readyQueue.Enqueue(pcb);
    }

    public PCB PickNext(uint coreId)
    {
        if (_readyQueue.TryDequeue(out PCB pcb))
            return pcb;

        return idlePcbs[coreId];
    }
}
