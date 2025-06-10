using ProcSim.Core.Process;
using System.Text;

namespace ProcSim.Core.Extensions;

public static class ProcessDtoSerializer
{
    extension(ProcessDto process)
    {
        public string SerializePretty()
        {
            StringBuilder sb = new();
            sb.AppendLine($"Processo: {process.Name}");
            sb.AppendLine();

            sb.AppendLine("Instruções:");
            for (int idx = 0; idx < process.Instructions.Count; idx++)
            {
                Instruction instr = process.Instructions[idx];
                sb.AppendLine($"{idx}. {instr.Mnemonic}");

                foreach (MicroOp microOp in instr.MicroOps)
                    sb.AppendLine($"• {microOp.Name}: {microOp.Description}");

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}