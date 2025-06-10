using ProcSim.Core.IO;
using ProcSim.Core.Process;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ProcSim.Core.Configuration;

public class ProcessConfigModel
{
    public ILoopConfig LoopConfig { get; set; }
    public string Name { get; set; }
    public List<IOperationConfigModel> Operations { get; set; }
    public ProcessStaticPriority Priority { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "operationConfig")]
[JsonDerivedType(typeof(CpuOperationConfigModel), "cpu")]
[JsonDerivedType(typeof(IoOperationConfigModel), "io")]
public interface IOperationConfigModel
{
    uint RepeatCount { get; set; }
}

public record CpuOperationConfigModel(CpuOperationType Type, int Min, int Max, uint RepeatCount) : IOperationConfigModel
{
    public uint RepeatCount { get; set; } = RepeatCount;
}

public enum CpuOperationType
{
    [Description("")]
    None,
    [Description("Random")]
    Random,
    [Description("ADD")]
    Add,
    [Description("SUB")]
    Subtract,
    [Description("MUL")]
    Multiply,
    [Description("DIV")]
    Divide,
}

public record IoOperationConfigModel : IOperationConfigModel
{
    public IoDeviceType DeviceType { get; set; }
    public uint Duration { get; set; }
    public uint MinDuration { get; set; }
    public uint MaxDuration { get; set; }
    public bool IsRandom { get; set; }
    public uint RepeatCount { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "loopConfig")]
[JsonDerivedType(typeof(InfiniteLoopConfig), "infinite")]
[JsonDerivedType(typeof(FiniteLoopConfig), "finite")]
[JsonDerivedType(typeof(RandomLoopConfig), "random")]
public interface ILoopConfig { }

public class InfiniteLoopConfig : ILoopConfig { }

public class FiniteLoopConfig : ILoopConfig
{
    public uint Iterations { get; set; }
}

public class RandomLoopConfig : ILoopConfig
{
    public uint MinIterations { get; set; }
    public uint MaxIterations { get; set; }
}