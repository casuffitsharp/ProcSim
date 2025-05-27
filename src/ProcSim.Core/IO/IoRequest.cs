using ProcSim.Core.Process;

namespace ProcSim.Core.IO;

public record IORequest(PCB Pcb, uint OperationUnits);
