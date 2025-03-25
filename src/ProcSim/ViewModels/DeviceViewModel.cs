using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Core.Enums;
using ProcSim.Core.IO.Devices;

namespace ProcSim.ViewModels;

public partial class DeviceViewModel : ObservableObject
{
    public IoDeviceType DeviceType { get; set; }

    public string Name { get; set; }

    [ObservableProperty]
    public partial bool IsEnabled { get; set; }

    [ObservableProperty]
    public partial int Channels { get; set; }

    public bool IsConfigurable => DeviceType is IoDeviceType.Disk or IoDeviceType.Memory;

    public IoDeviceConfig MapToDeviceConfig()
    {
        return new()
        {
            Name = Name,
            DeviceType = DeviceType,
            Channels = Channels,
            IsEnabled = IsEnabled
        };
    }

    public void UpdateFromDeviceConfig(IoDeviceConfig config)
    {
        Name = config.Name;
        DeviceType = config.DeviceType;
        Channels = config.Channels;
        IsEnabled = config.IsEnabled;
    }
}