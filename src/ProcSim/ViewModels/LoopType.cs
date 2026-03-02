using System.ComponentModel;

namespace ProcSim.ViewModels;

public enum LoopType
{
    [Description("")]
    None,
    [Description("Infinito")]
    Infinite,
    [Description("Finito")]
    Finite,
    [Description("Random")]
    Random
}
