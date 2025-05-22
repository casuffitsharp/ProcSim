using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace ProcSim.New.ViewModels;

public class DeviceSettingViewModel : ObservableObject
{
    public string DeviceName { get; set; }
    public uint Channels { get; set; }
    public bool IsEnabled { get; set; }
}

public class VMSettingsViewModel : ObservableObject
{
    public int Cores { get; set; }
    public int Quantum { get; set; }

    public ObservableCollection<DeviceSettingViewModel> Devices { get; } = [];
}