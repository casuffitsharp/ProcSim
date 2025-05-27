using System.ComponentModel;

namespace ProcSim.Core.Old.Enums;

public enum SchedulingAlgorithmType
{
    [Description("")]
    None,

    [Description("FCFS")]
    Fcfs,

    [Description("Round Robin")]
    RoundRobin
}
