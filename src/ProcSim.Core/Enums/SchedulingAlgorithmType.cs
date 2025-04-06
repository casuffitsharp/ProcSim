using System.ComponentModel;

namespace ProcSim.Core.Enums;

public enum SchedulingAlgorithmType
{
    [Description("")]
    None,

    [Description("FCFS")]
    Fcfs,

    [Description("Round Robin")]
    RoundRobin
}
