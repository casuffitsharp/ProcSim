using System.ComponentModel;

namespace ProcSim.Core.Process;

public enum ProcessStaticPriority
{
    [Description("Baixa")]
    Low = 5,
    [Description("Abaixo do Normal")]
    BelowNormal = 14,
    [Description("Normal")]
    Normal = 23,
    [Description("Acima do Normal")]
    AboveNormal = 32,
    [Description("Alta")]
    High = 41,
    [Description("Tempo Real")]
    RealTime = 50
}