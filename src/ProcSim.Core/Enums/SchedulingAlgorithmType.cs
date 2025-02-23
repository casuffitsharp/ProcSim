using System.ComponentModel;

namespace ProcSim.Core.Enums;

public enum SchedulingAlgorithmType
{
    [Description("FCFS")]
    Fcfs,

    [Description("Round Robin")]
    RoundRobin
}
