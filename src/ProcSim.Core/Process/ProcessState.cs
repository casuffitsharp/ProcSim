using System.ComponentModel;

namespace ProcSim.Core.Process;

public enum ProcessState
{
    [Description("Novo")]
    New,
    [Description("Pronto")]
    Ready,
    [Description("Em Execução")]
    Running,
    [Description("Bloqueado")]
    Waiting,
    [Description("Concluído")]
    Terminated
}
