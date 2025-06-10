using System.ComponentModel;

namespace ProcSim.Core.Scheduler;

public enum SchedulerType
{
    [Description("")]
    None,

    [Description("Round Robin")]
    RoundRobin,

    [Description("Prioridade")]
    Priority,
}
