namespace ProcSim.Core.Logging;

public interface ILogger
{
    void Log(LogEvent logEvent);
    void Log(string message);
}
