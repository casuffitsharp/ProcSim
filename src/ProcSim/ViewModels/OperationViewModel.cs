using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Converters;
using ProcSim.Core.Models.Operations;
using ProcSim.Core.New.IO;

namespace ProcSim.ViewModels;

public partial class OperationViewModel() : ObservableObject
{
    public OperationViewModel(IOperation operation) : this()
    {
        Model = operation;
        Model.RemainingTimeChanged += OnRemainingTimeChanged;
        Model.ChannelChanged += OnChannelChanged;
        Reset();
    }

    public IOperation Model { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges), nameof(IsValid))]
    public partial int Duration { get; set; }

    public int RemainingTime => Model.RemainingTime;
    private int? Channel => Model.Channel;

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
                OnPropertyChanged(nameof(Type));
            }
        }
    } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges), nameof(IsValid), nameof(Type))]
    public partial IoDeviceType IoDeviceType { get; set; }

    public string Type
    {
        get
        {
            string prefix = IsCpu ? "CPU" : new EnumDescriptionConverter().Convert(IoDeviceType, typeof(string)) as string;
            string suffix = Channel.HasValue ? $"({Channel})" : string.Empty;
            return $"{prefix}{suffix}";
        }
    }

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

    private void OnRemainingTimeChanged()
    {
        OnPropertyChanged(nameof(RemainingTime));
        if (RemainingTime > Duration - 4)
            OnPropertyChanged(nameof(Type));
    }

    private void OnChannelChanged()
    {
        OnPropertyChanged(nameof(Type));
    }
}
