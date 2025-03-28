﻿using ProcSim.Core.Enums;
using ProcSim.Core.IO.Devices;

namespace ProcSim.Core.Configuration;

public record VmConfig
{
    public List<IoDeviceConfig> Devices { get; set; }
    public SchedulingAlgorithmType SchedulingAlgorithmType { get; set; }
}
