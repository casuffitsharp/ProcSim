using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Core.Enums;
using ProcSim.Core.IO.Devices;

namespace ProcSim.ViewModels;

public partial class DeviceViewModel : ObservableObject
{
    public IoDeviceType DeviceType { get; set; }
    public string Name { get; set; }

    /// <summary>
    /// Indica se o dispositivo está selecionado para uso na VM.
    /// </summary>
    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    /// <summary>
    /// Propriedade de configuração, ex.: quantidade de canais.
    /// Será utilizada apenas para dispositivos que permitem customização (ex.: disco e memória).
    /// </summary>
    [ObservableProperty]
    public partial int Channels { get; set; }

    /// <summary>
    /// Indica se o dispositivo permite customização (por exemplo, disco e memória).
    /// </summary>
    public bool IsConfigurable => DeviceType is IoDeviceType.Disk or IoDeviceType.Memory;

    public IoDeviceConfig MapToDeviceConfig()
    {
        return new()
        {
            Name = Name,
            DeviceType = DeviceType,
            Channels = Channels
        };
    }
}