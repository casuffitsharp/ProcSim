namespace ProcSim.Core.IO.Strategies;

public interface IIoSchedulingStrategy
{
    IReadOnlyList<IoRequest> OrderRequests(IEnumerable<IoRequest> requests);
}
