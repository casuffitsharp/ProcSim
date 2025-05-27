using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Core.IO;

namespace ProcSim.ViewModels;

public class IoDeviceConfigViewModel : ObservableObject
{
    public IoDeviceConfigViewModel() { }

    public IoDeviceConfigViewModel(IoDeviceConfigModel model)
    {
        UpdateFromModel(model);
    }

    public string Name { get; set; }
    public uint Channels { get; set; }
    public uint BaseLatency { get; set; }
    public IoDeviceType Type { get; set; }
    public bool IsEnabled { get; set; }

    public IoDeviceConfigModel MapToModel()
    {
        return new IoDeviceConfigModel(Type, Name, BaseLatency, Channels, IsEnabled);
    }

    public void UpdateFromModel(IoDeviceConfigModel model)
    {
        Name = model.Name;
        Channels = model.Channels;
        BaseLatency = model.BaseLatency;
        Type = model.Type;
        IsEnabled = model.IsEnabled;
    }
}
