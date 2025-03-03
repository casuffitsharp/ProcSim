using ProcSim.Core.Enums;
using ProcSim.Core.Models;

namespace ProcSim.Core.IO;

public sealed record IoRequest(Process Process, int IoTime, IoDeviceType DeviceType, DateTime ArrivalTime);
