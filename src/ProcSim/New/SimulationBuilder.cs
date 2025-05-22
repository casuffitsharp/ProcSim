using ProcSim.Core.Models.Operations;
using ProcSim.Core.New.Process;
using ProcSim.New.ViewModels;

namespace ProcSim.New;

/// <summary>
/// Constrói instâncias de PCB + lista de instruções,
/// materializando fixed/random e alocando canais em runtime.
/// </summary>
public class SimulationBuilder
{
    private readonly IReadOnlyList<ProcessViewModel> _processDefs;
    private readonly IReadOnlyDictionary<DeviceType, DeviceSettingViewModel> _deviceSettings;
    private readonly IRandomProvider _random;

    public SimulationBuilder(IEnumerable<ProcessViewModel> processDefs, IEnumerable<DeviceSettingViewModel> deviceSettings, IRandomProvider random)
    {
        _processDefs = processDefs.ToList();
        _deviceSettings = deviceSettings.Where(d => d.IsEnabled).ToDictionary(d => d.DeviceType);
        _random = random;
    }

    /// <summary>
    /// Gera todos os PCBs prontos para admitir no scheduler.
    /// </summary>
    public IEnumerable<PCB> BuildAll()
    {
        foreach (var pd in _processDefs)
        {
            int runs = pd.ExecMode switch
            {
                ExecutionMode.Fixed => pd.FixedCount,
                ExecutionMode.Random => _random.Next(pd.MinCount, pd.MaxCount),
                ExecutionMode.Infinite => int.MaxValue,
                _ => pd.FixedCount
            };

            for (int i = 0; i < runs; i++)
            {
                // cria PCB com PID único e prioridade
                var pid = GeneratePid(pd.Id, i);
                var pcb = new PCB((uint)pid, /* registers iniciais */ null)
                {
                    Priority = (uint)pd.Priority
                };

                // monta instruções conforme definições
                foreach (var ivm in pd.Instructions)
                    foreach (var op in BuildInstruction(ivm))
                        pcb.Operations.Add(op);

                yield return pcb;

                if (pd.ExecMode == ExecutionMode.Infinite)
                    yield break;
            }
        }
    }

    private IEnumerable<IOperation> BuildInstruction(InstructionViewModel ivm)
    {
        if (ivm.IsCpu)
        {
            var type = ivm.CpuOp == InstructionType.Random ? _random.NextEnum<InstructionType>() : ivm.CpuOp;

            // supondo duration = 1 para cada CPU-op
            yield return new CpuOperation(duration: 1, opType: type);
        }
        else
        {
            // calcula duração
            int duration = ivm.IoMode == DurationMode.Fixed ? ivm.IoFixed : _random.Next(ivm.IoMin, ivm.IoMax);

            // escolhe device settings
            var ds = _deviceSettings[ivm.SelectedDevice.DeviceType];
            uint channels = ds.Channels;
            var devType = ds.DeviceType;

            yield return new IoOperation(durationUnits: (uint)duration, deviceType: devType
            );
        }
    }

    private int GeneratePid(string baseId, int index)
    {
        // ex.: "P1" -> P1_1, P1_2, ou "R2" já traz prefixo R
        var suffix = (index + 1).ToString();
        var key = baseId.StartsWith("R") ? $"{baseId}_{suffix}" : $"{baseId}{suffix}";
        // hash para uint positivo
        return key.GetHashCode() & 0x7FFFFFFF;
    }
}

/// <summary>
/// Contrato para geração de números aleatórios/testáveis.
/// </summary>
public interface IRandomProvider
{
    int Next(int minInclusive, int maxInclusive);
    T NextEnum<T>() where T : Enum;
}

public class DefaultRandomProvider : IRandomProvider
{
    private readonly Random _rnd = new();
    public int Next(int minInclusive, int maxInclusive) => _rnd.Next(minInclusive, maxInclusive + 1);
    public T NextEnum<T>() where T : Enum
    {
        var vals = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        return vals[_rnd.Next(vals.Length)];
    }
}
