using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Converters;
using ProcSim.Core.Configuration;
using ProcSim.Core.IO;
using System.ComponentModel;

namespace ProcSim.ViewModels;

public enum OperationType
{
    [Description("")]
    None,
    [Description("CPU")]
    Cpu,
    [Description("I/O")]
    Io
}

public partial class OperationConfigViewModel : ObservableObject
{
    public OperationConfigViewModel()
    {
        CpuOperationConfig.PropertyChanged += Child_PropertyChanged;
        IoOperationConfig.PropertyChanged += Child_PropertyChanged;

        if (Type == OperationType.None)
            Type = OperationType.Cpu;
    }

    public OperationConfigViewModel(IOperationConfigModel model) : this()
    {
        UpdateFromModel(model);
    }

    public static OperationType[] OperationTypeValues { get; } = [.. Enum.GetValues<OperationType>().Where(x => x != OperationType.None)];

    public CpuOperationConfigViewModel CpuOperationConfig { get; set; } = new();
    public IoOperationConfigViewModel IoOperationConfig { get; set; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Summary), nameof(IsCpu))]
    public partial OperationType Type { get; set; }

    public bool IsCpu => Type == OperationType.Cpu;

    public string Summary => Type switch
    {
        OperationType.Cpu => CpuOperationConfig.GetSummary(),
        OperationType.Io => IoOperationConfig.GetSummary(),
        _ => string.Empty,
    };

    public IOperationConfigModel MapToModel()
    {
        return Type switch
        {
            OperationType.Cpu => CpuOperationConfig.MapToModel(),
            OperationType.Io => IoOperationConfig.MapToModel(),
            _ => throw new NotImplementedException(),
        };
    }

    public void UpdateFromModel(IOperationConfigModel model)
    {
        switch (model)
        {
            case CpuOperationConfigModel cpuOperationConfigModel:
                Type = OperationType.Cpu;
                CpuOperationConfig.UpdateFromModel(cpuOperationConfigModel);
                break;
            case IoOperationConfigModel ioOperationConfigModel:
                Type = OperationType.Io;
                IoOperationConfig.UpdateFromModel(ioOperationConfigModel);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public OperationConfigViewModel Copy()
    {
        return new(MapToModel());
    }

    public void UpdateFromViewModel(OperationConfigViewModel other)
    {
        UpdateFromModel(other.MapToModel());
    }

    public bool Validate(out IEnumerable<string> errors)
    {
        List<string> list = [];

        if (Type == OperationType.None)
            list.Add("Tipo de operação não foi selecionado.");

        if (Type == OperationType.Cpu && !CpuOperationConfig.Validate(out IEnumerable<string> cpuErrors))
            list.AddRange(cpuErrors);
        else if (Type == OperationType.Io && !IoOperationConfig.Validate(out IEnumerable<string> ioErrors))
            list.AddRange(ioErrors);

        errors = list;
        return list.Count == 0;
    }

    public bool Equals(OperationConfigViewModel other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        if (Type != other.Type) return false;
        if (CpuOperationConfig.Equals(other.CpuOperationConfig) == false) return false;
        if (IoOperationConfig.Equals(other.IoOperationConfig) == false) return false;

        return true;
    }

    private void Child_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(Summary));
    }
}

public partial class CpuOperationConfigViewModel : ObservableObject
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

    public bool Equals(CpuOperationConfigViewModel other)
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

public partial class IoOperationConfigViewModel : ObservableObject
{
    public IoOperationConfigViewModel()
    {
        DeviceType = IoDeviceType.Disk;
        RepeatCount = 1;
    }

    public static IoDeviceType[] IoDeviceTypeValues { get; } = [.. Enum.GetValues<IoDeviceType>().Where(x => x != IoDeviceType.None)];

    [ObservableProperty]
    public partial IoDeviceType DeviceType { get; set; }
    [ObservableProperty]
    public partial uint Duration { get; set; }
    [ObservableProperty]
    public partial uint MinDuration { get; set; }
    [ObservableProperty]
    public partial uint MaxDuration { get; set; }
    [ObservableProperty]
    public partial bool IsRandom { get; set; }
    [ObservableProperty]
    public partial uint RepeatCount { get; set; }

    public bool Validate(out IEnumerable<string> errors)
    {
        List<string> list = [];

        if (DeviceType == IoDeviceType.None)
            list.Add("Dispositivo não foi selecionado.");

        if (Duration == 0 && !IsRandom)
            list.Add("Duração não pode ser zero.");

        if (IsRandom && (MinDuration == 0 || MaxDuration == 0 || MinDuration > MaxDuration))
            list.Add("Duração mínima e máxima inválida.");

        if (RepeatCount == 0)
            list.Add("Repetições não podem ser zero.");

        errors = list;
        return list.Count == 0;
    }

    public string GetSummary()
    {
        if (IsRandom)
            return $"I/O: {RepeatCount}x {EnumDescriptionConverter.GetEnumDescription(DeviceType),-6} Min: {MinDuration}ms Max: {MaxDuration}ms";

        return $"I/O: {RepeatCount}x {EnumDescriptionConverter.GetEnumDescription(DeviceType),-6} {Duration}ms";
    }

    public IoOperationConfigModel MapToModel()
    {
        return new()
        {
            DeviceType = DeviceType,
            Duration = Duration,
            MinDuration = MinDuration,
            MaxDuration = MaxDuration,
            IsRandom = IsRandom,
            RepeatCount = RepeatCount
        };
    }

    public void UpdateFromModel(IoOperationConfigModel model)
    {
        DeviceType = model.DeviceType;
        Duration = model.Duration;
        MinDuration = model.MinDuration;
        MaxDuration = model.MaxDuration;
        IsRandom = model.IsRandom;
        RepeatCount = model.RepeatCount;
    }

    public bool Equals(IoOperationConfigViewModel other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        if (DeviceType != other.DeviceType) return false;
        if (Duration != other.Duration) return false;
        if (MinDuration != other.MinDuration) return false;
        if (MaxDuration != other.MaxDuration) return false;
        if (IsRandom != other.IsRandom) return false;
        if (RepeatCount != other.RepeatCount) return false;

        return true;
    }
}