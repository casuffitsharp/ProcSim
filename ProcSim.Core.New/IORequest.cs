namespace ProcSim.Core.New;

// Estrutura de requisição de I/O
public record IORequest(PCB Pcb, uint DeviceId, uint OperationUnits);
