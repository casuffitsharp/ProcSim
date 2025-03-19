using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Core.Enums;
using ProcSim.Core.Models.Operations;

namespace ProcSim.ViewModels;

public partial class OperationViewModel() : ObservableObject
{
    public OperationViewModel(IOperation operation) : this()
    {
        Model = operation;
        Reset();
    }

    public IOperation Model { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges), nameof(IsValid))]
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
            if (SetProperty(ref field, value))
            {
                IoDeviceType = IoDeviceType.None;
                OnPropertyChanged(nameof(HasChanges));
                OnPropertyChanged(nameof(IsValid));
            }
        }
    } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges), nameof(IsValid))]
    public partial IoDeviceType IoDeviceType { get; set; }

    public OperationViewModel Commit()
    {
        IOperation operation = IsCpu ? new CpuOperation(Duration) : new IoOperation(Duration, IoDeviceType);
        return new(operation);
    }

    public void Reset()
    {
        Duration = Model?.Duration ?? 0;
        IsCpu = Model is null or ICpuOperation;
        IoDeviceType = Model is IIoOperation ioOperation ? ioOperation.DeviceType : IoDeviceType.None;
    }

    public bool HasChanges => Duration != Model?.Duration || IsCpu != (Model is null or ICpuOperation) || IoDeviceType != (Model is IIoOperation ioOperation ? ioOperation.DeviceType : IoDeviceType.None);
    
    public bool IsValid => Duration > 0 && (IsCpu || IoDeviceType is not IoDeviceType.None);
}
