using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Core.Configuration;
using System.ComponentModel;

namespace ProcSim.ViewModels;

public sealed partial class OperationConfigViewModel : ObservableObject
{
    public OperationConfigViewModel()
    {
        CpuOperationConfig.PropertyChanged += Child_PropertyChanged;
        IoOperationConfig.PropertyChanged += Child_PropertyChanged;

        if (Type == OperationType.None)
            Type = OperationType.Cpu;
    }

    public OperationConfigViewModel(IOperationConfigModel model) : this()
    {
        UpdateFromModel(model);
    }

    public static OperationType[] OperationTypeValues { get; } = [.. Enum.GetValues<OperationType>().Where(x => x != OperationType.None)];

    public CpuOperationConfigViewModel CpuOperationConfig { get; set; } = new();
    public IoOperationConfigViewModel IoOperationConfig { get; set; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Summary), nameof(IsCpu))]
    public partial OperationType Type { get; set; }

    public bool IsCpu => Type == OperationType.Cpu;

    public string Summary => Type switch
    {
        OperationType.Cpu => CpuOperationConfig.GetSummary(),
        OperationType.Io => IoOperationConfig.GetSummary(),
        _ => string.Empty,
    };

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

    public bool Validate(out IEnumerable<string> errors)
    {
        List<string> list = [];

        if (Type == OperationType.None)
            list.Add("Tipo de operação não foi selecionado.");

        if (Type == OperationType.Cpu && !CpuOperationConfig.Validate(out IEnumerable<string> cpuErrors))
            list.AddRange(cpuErrors);
        else if (Type == OperationType.Io && !IoOperationConfig.Validate(out IEnumerable<string> ioErrors))
            list.AddRange(ioErrors);

        errors = list;
        return list.Count == 0;
    }

    public bool ValueEquals(OperationConfigViewModel other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        if (Type != other.Type) return false;
        if (!CpuOperationConfig.ValueEquals(other.CpuOperationConfig)) return false;
        if (!IoOperationConfig.ValueEquals(other.IoOperationConfig)) return false;

        return true;
    }

    private void Child_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(Summary));
    }
}
