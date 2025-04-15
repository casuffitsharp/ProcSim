using ProcSim.Core.Enums;
using System.Text.Json.Serialization;

namespace ProcSim.Core.Logging;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "EventDiscriminator")]
[JsonDerivedType(typeof(ProcessStateChangeEvent), nameof(ProcessStateChangeEvent))]
[JsonDerivedType(typeof(IoDeviceStateChangeEvent), nameof(IoDeviceStateChangeEvent))]
[JsonDerivedType(typeof(CpuConfigurationChangeEvent), nameof(CpuConfigurationChangeEvent))]
[JsonDerivedType(typeof(DeviceConfigurationChangeEvent), nameof(DeviceConfigurationChangeEvent))]
public abstract class SimEvent
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int Channel { get; set; }
    public string Message { get; set; }
    // public Dictionary<string, object> AdditionalData { get; set; } = new();
}

public class ProcessStateChangeEvent : SimEvent
{
    public int ProcessId { get; set; }
    public ProcessState NewState { get; set; }
}

public class IoDeviceStateChangeEvent : SimEvent
{
    public string Device { get; set; }
    public bool IsActive { get; set; }
}

public class CpuConfigurationChangeEvent : SimEvent
{
    public int OldCpuCount { get; set; }
    public int NewCpuCount { get; set; }
}

public class DeviceConfigurationChangeEvent : SimEvent
{
    public string Device { get; set; }
    public bool IsAdded { get; set; }
}
