namespace ProcSim.Core.Monitoring.Models;

public record IoRequestNotification(uint Pid, uint DeviceId, uint Channel);