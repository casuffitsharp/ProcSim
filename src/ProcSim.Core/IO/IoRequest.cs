using ProcSim.Core.Process;

namespace ProcSim.Core.IO;

public record IORequest(Pcb Pcb, uint OperationUnits);
