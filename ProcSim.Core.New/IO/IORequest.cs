using ProcSim.Core.New.Process;

namespace ProcSim.Core.New.IO;

public record IORequest(PCB Pcb, uint OperationUnits);
