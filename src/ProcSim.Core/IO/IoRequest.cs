using ProcSim.Core.Models;
using ProcSim.Core.Models.Operations;

namespace ProcSim.Core.IO;

public sealed record IoRequest(Process Process, IoOperation Operation, DateTime ArrivalTime);
