using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Converters;
using ProcSim.Core.Configuration;
using ProcSim.Core.IO;

namespace ProcSim.ViewModels;

public sealed partial class IoOperationConfigViewModel : ObservableObject
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
            return $"I/O: {RepeatCount}x {EnumDescriptionConverter.GetEnumDescription(DeviceType),-6} Min: {MinDuration}u Max: {MaxDuration}u";

        return $"I/O: {RepeatCount}x {EnumDescriptionConverter.GetEnumDescription(DeviceType),-6} {Duration}u";
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

    public bool ValueEquals(IoOperationConfigViewModel other)
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