using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDesignThemes.Wpf.Converters;
using ProcSim.Core.Enums;
using ProcSim.Core.Models.Operations;

namespace ProcSim.ViewModels;

public partial class OperationViewModel : ObservableObject
{
    public IOperation Model { get; }

    public OperationViewModel(IOperation operation)
    {
        Model = operation;
        Reset();
    }

    [ObservableProperty]
    public partial int Duration { get; set; }

    [ObservableProperty]
    public partial int RemainingTime { get; set; }

    [ObservableProperty]
    public partial bool IsCompleted { get; set; }

    public bool IsCpu
    {
        get => field;
        set
        {
            if (SetProperty(ref field, value) && value)
            {
                IoDeviceType = IoDeviceType.None;
                OnPropertyChanged(nameof(IoDeviceType));
            }
        }
    }

    [ObservableProperty]
    public partial IoDeviceType IoDeviceType { get; set; }

    public OperationViewModel Commit()
    {
        IOperation operation = IsCpu ? new CpuOperation(Duration) : new IoOperation(Duration, IoDeviceType);
        return new(operation);
    }

    public void Reset()
    {
        Duration = Model.Duration;
        IsCpu = Model is ICpuOperation;
        IoDeviceType = Model is IIoOperation ioOperation ? ioOperation.DeviceType : IoDeviceType.None;
    }
}
