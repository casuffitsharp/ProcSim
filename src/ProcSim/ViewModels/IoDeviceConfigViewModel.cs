using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Core.IO;

namespace ProcSim.ViewModels;

public partial class IoDeviceConfigViewModel : ObservableObject
{
    public IoDeviceConfigViewModel()
    {
        Name = string.Empty;
        Channels = 1;
        BaseLatency = 1000;
        IsEnabled = false;
    }

    public IoDeviceConfigViewModel(IoDeviceConfigModel model)
    {
        UpdateFromModel(model);
    }

    public string Name { get; set; }

    [ObservableProperty]
    public partial uint Channels { get; set; }
    [ObservableProperty]
    public partial uint BaseLatency { get; set; }
    [ObservableProperty]
    public partial IoDeviceType Type { get; set; }
    [ObservableProperty]
    public partial bool IsEnabled { get; set; }

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

    public bool Validate(out List<string> errors)
    {
        errors = [];

        if (Channels == 0)
            errors.Add("Quantidade de canais deve ser maior que 0.");

        if (BaseLatency == 0)
            errors.Add("Latência deve ser maior que 0.");

        return errors.Count == 0;
    }
}
