using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Core.Configuration;
using ProcSim.Core.IO;
using System;
using System.ComponentModel;
using System.Windows;

namespace ProcSim.ViewModels;

public enum OperationType
{
    [Description("")]
    None,
    [Description("CPU")]
    Cpu,
    [Description("IO")]
    Io
}

public partial class OperationConfigViewModel : ObservableObject, IEquatable<OperationConfigViewModel>
{
    public OperationConfigViewModel() { }
    
    public OperationConfigViewModel(IOperationConfigModel model)
    {
        UpdateFromModel(model);
    }

    public CpuOperationConfigViewModel CpuOperationConfig { get; set; } = new();
    public IoOperationConfigViewModel IoOperationConfig { get; set; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCpu))]
    public partial OperationType Type { get; set; }

    public bool IsCpu => Type == OperationType.Cpu;

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

    public bool IsValid
    {
        get
        {
            if (Type == OperationType.None) return false;
            if (Type == OperationType.Cpu && !CpuOperationConfig.IsValid) return false;
            if (Type == OperationType.Io && !IoOperationConfig.IsValid) return false;

            return true;
        }
    }

    public override bool Equals(object obj)
    {
        return obj is OperationConfigViewModel other && Equals(other);
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
}

public partial class CpuOperationConfigViewModel : ObservableObject, IEquatable<CpuOperationConfigViewModel>
{
    public CpuOperationType Type { get; set; }
    public int R1 { get; set; }
    public int R2 { get; set; }

    public CpuOperationConfigModel MapToModel() => new(Type, R1, R2);

    public void UpdateFromModel(CpuOperationConfigModel model)
    {
        Type = model.Type;
        R1 = model.R1;
        R2 = model.R2;
    }

    public bool IsValid
    {
        get
        {
            if (Type == CpuOperationType.None) return false;
            if (Type is CpuOperationType.Divide or CpuOperationType.Random && R2 == 0) return false;

            return true;
        }
    }

    public override bool Equals(object obj)
    {
        return obj is CpuOperationConfigViewModel other && Equals(other);
    }

    public bool Equals(CpuOperationConfigViewModel other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        if (Type != other.Type) return false;
        if (R1 != other.R1) return false;
        if (R2 != other.R2) return false;
        
        return true;
    }
}

public partial class IoOperationConfigViewModel : ObservableObject, IEquatable<IoOperationConfigViewModel>
{
    public IoDeviceType DeviceType { get; set; }
    public uint Duration { get; set; }
    public uint MinDuration { get; set; }
    public uint MaxDuration { get; set; }
    public bool IsRandom { get; set; }

    public IoOperationConfigModel MapToModel()
    {
        return new()
        {
            DeviceType = DeviceType,
            Duration = Duration,
            MinDuration = MinDuration,
            MaxDuration = MaxDuration,
            IsRandom = IsRandom
        };
    }

    public void UpdateFromModel(IoOperationConfigModel model)
    {
        DeviceType = model.DeviceType;
        Duration = model.Duration;
        MinDuration = model.MinDuration;
        MaxDuration = model.MaxDuration;
        IsRandom = model.IsRandom;
    }

    public bool IsValid
    {
        get
        {
            if (DeviceType == IoDeviceType.None) return false;
            if (Duration == 0 && !IsRandom) return false;
            if (IsRandom && (MinDuration == 0 || MaxDuration == 0 || MinDuration > MaxDuration)) return false;

            return true;
        }
    }

    public override bool Equals(object obj)
    {
        return obj is IoOperationConfigViewModel other && Equals(other);
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
        
        return true;
    }
}