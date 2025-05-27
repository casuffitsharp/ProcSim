using ProcSim.Core.Old.IO;

namespace ProcSim.Core.Old.IO.Strategies;

public interface IIoSchedulingStrategy
{
    IReadOnlyList<IoRequest> OrderRequests(IEnumerable<IoRequest> requests);
}
