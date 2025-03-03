namespace ProcSim.Core.IO.Strategies;

public sealed class FifoIoSchedulingStrategy : IIoSchedulingStrategy
{
    public IReadOnlyList<IoRequest> OrderRequests(IEnumerable<IoRequest> requests)
    {
        return [.. requests.OrderBy(r => r.ArrivalTime)];
    }
}
