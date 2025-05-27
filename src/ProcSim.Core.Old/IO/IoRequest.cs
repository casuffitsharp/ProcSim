using ProcSim.Core.Old.Models;
using ProcSim.Core.Old.Models.Operations;

namespace ProcSim.Core.Old.IO;

public sealed record IoRequest(Process Process, IoOperation Operation, DateTime ArrivalTime);
