namespace ProcSim.Core.Monitoring.Models;

public record IoRequestNotification(int Pid, uint DeviceId, uint Channel);