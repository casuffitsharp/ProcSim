namespace ProcSim.Core.Old.Logging;

public interface IStructuredLogger
{
    void Log(SimEvent simEvent);
    event Action<SimEvent> OnLog;
    IEnumerable<SimEvent> GetAllEvents();
}
