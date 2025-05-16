using System.Collections.Concurrent;

namespace ProcSim.Core.New.Interruptions;

public class InterruptController
{
    private readonly ConcurrentDictionary<uint, ConcurrentQueue<uint>> _queues = new();
    private readonly ConcurrentDictionary<uint, uint[]> _redirectTable = new();

    public void RegisterCore(uint coreId)
    {
        _queues[coreId] = new ConcurrentQueue<uint>();
    }

    public void ConfigureRedirection(uint vector, params uint[] coreIds)
    {
        _redirectTable[vector] = coreIds;
    }

    public void RaiseExternal(uint vector)
    {
        int coreId = Random.Shared.Next(0, _queues.Count);
        _queues[(uint)coreId].Enqueue(vector);
    }

    public void RaiseLocal(uint vector, uint coreId)
    {
        _queues[coreId].Enqueue(vector);
    }

    public uint? FetchReady(uint coreId)
    {
        if (_queues[coreId].TryDequeue(out uint vector))
            return vector;

        return null;
    }
}
