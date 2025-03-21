using System.Text.Json.Serialization;

namespace ProcSim.Core.Models.Operations;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "operationType")]
[JsonDerivedType(typeof(CpuOperation), "cpu")]
[JsonDerivedType(typeof(IoOperation), "io")]
public interface IOperation
{
    int Duration { get; }
    int RemainingTime { get; }
    bool IsCompleted { get; }
    void ExecuteTick();
    void Reset();
}
