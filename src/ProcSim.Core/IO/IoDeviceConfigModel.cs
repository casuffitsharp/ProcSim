namespace ProcSim.Core.IO;

public record IoDeviceConfigModel(IoDeviceType Type, string Name, uint BaseLatency, uint Channels, bool IsEnabled) { }
