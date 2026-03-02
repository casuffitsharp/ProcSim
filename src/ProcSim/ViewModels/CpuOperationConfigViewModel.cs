using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Converters;
using ProcSim.Core.Configuration;

namespace ProcSim.ViewModels;

public sealed partial class CpuOperationConfigViewModel : ObservableObject
{
    public CpuOperationConfigViewModel()
    {
        Type = CpuOperationType.Random;
        RepeatCount = 1;
    }

    public static CpuOperationType[] CpuOperationTypeValues { get; } = [.. Enum.GetValues<CpuOperationType>().Where(x => x != CpuOperationType.None)];

    [ObservableProperty]
    public partial CpuOperationType Type { get; set; }
    [ObservableProperty]
    public partial int Min { get; set; }
    [ObservableProperty]
    public partial int Max { get; set; }
    [ObservableProperty]
    public partial uint RepeatCount { get; set; }

    public CpuOperationConfigModel MapToModel()
    {
        return new(Type, Min, Max, RepeatCount);
    }

    public void UpdateFromModel(CpuOperationConfigModel model)
    {
        Type = model.Type;
        Min = model.Min;
        Max = model.Max;
        RepeatCount = model.RepeatCount;
    }

    public bool Validate(out IEnumerable<string> errors)
    {
        List<string> list = [];

        if (Type == CpuOperationType.None)
            list.Add("Tipo de operação CPU não foi selecionado.");

        if (Min == 0 || Max == 0)
            list.Add("Valores mínimo e máximo não podem ser zero.");

        if (Min >= Max)
            list.Add("Valor mínimo não pode ser maior ou igual ao máximo.");

        if (RepeatCount == 0)
            list.Add("Repetições não podem ser zero.");

        errors = list;
        return list.Count == 0;
    }

    public string GetSummary()
    {
        return $"CPU: {RepeatCount}x {EnumDescriptionConverter.GetEnumDescription(Type),-6} Min: {Min} Max: {Max}";
    }

    public bool ValueEquals(CpuOperationConfigViewModel other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        if (Type != other.Type) return false;
        if (Min != other.Min) return false;
        if (Max != other.Max) return false;
        if (RepeatCount != other.RepeatCount) return false;

        return true;
    }
}
