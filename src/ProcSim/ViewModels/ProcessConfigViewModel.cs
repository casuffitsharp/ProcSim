using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcSim.Core.Configuration;
using ProcSim.Core.Process;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ProcSim.ViewModels;

public enum LoopType
{
    [Description("")]
    None,
    [Description("Infinito")]
    Infinite,
    [Description("Finito")]
    Finite,
    [Description("Random")]
    Random
}

public partial class ProcessConfigViewModel : ObservableObject//, IDropTarget
{
    public ProcessConfigViewModel()
    {
        LoopType = LoopType.Infinite;
        Priority = ProcessStaticPriority.Normal;
        AddOperationCommand = new RelayCommand(AddOperation);
        RemoveOperationCommand = new RelayCommand<OperationConfigViewModel>(RemoveOperation);
    }

    public ProcessConfigViewModel(ProcessConfigModel model) : this()
    {
        UpdateFromModel(model);
    }

    public static LoopType[] LoopTypeValues { get; } = [.. Enum.GetValues<LoopType>().Where(x => x != LoopType.None)];
    public static ProcessStaticPriority[] PriorityValues { get; } = [.. Enum.GetValues<ProcessStaticPriority>()];

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial ProcessStaticPriority Priority { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRandomLoop), nameof(IsFiniteLoop))]
    public partial LoopType LoopType { get; set; }

    [ObservableProperty]
    public partial uint Iterations { get; set; }

    [ObservableProperty]
    public partial uint MinIterations { get; set; }

    [ObservableProperty]
    public partial uint MaxIterations { get; set; }

    public ObservableCollection<OperationConfigViewModel> Operations { get; set; } = [];

    [ObservableProperty]
    public partial OperationConfigViewModel SelectedOperation { get; set; }

    public bool IsRandomLoop => LoopType == LoopType.Random;
    public bool IsFiniteLoop => LoopType == LoopType.Finite;

    public IRelayCommand AddOperationCommand { get; }
    public IRelayCommand<OperationConfigViewModel> RemoveOperationCommand { get; }

    public ProcessConfigModel MapToModel()
    {
        return new()
        {
            Priority = Priority,
            Name = Name,
            Operations = [.. Operations.Select(op => op.MapToModel())],
            LoopConfig = LoopType switch
            {
                LoopType.Infinite => new InfiniteLoopConfig(),
                LoopType.Finite => new FiniteLoopConfig { Iterations = Iterations },
                LoopType.Random => new RandomLoopConfig { MinIterations = MinIterations, MaxIterations = MaxIterations },
                _ => throw new NotImplementedException()
            }
        };
    }

    public void UpdateFromViewModel(ProcessConfigViewModel other)
    {
        if (other is null) return;
        UpdateFromModel(other.MapToModel());
    }

    public void UpdateFromModel(ProcessConfigModel model)
    {
        Name = model.Name;
        Priority = model.Priority;
        Operations = [.. model.Operations.Select(op => new OperationConfigViewModel(op))];

        switch (model.LoopConfig)
        {
            case InfiniteLoopConfig:
            {
                LoopType = LoopType.Infinite;
                break;
            }
            case FiniteLoopConfig finiteLoopConfig:
            {
                LoopType = LoopType.Finite;
                Iterations = finiteLoopConfig.Iterations;
                break;
            }
            case RandomLoopConfig randomLoopConfig:
            {
                LoopType = LoopType.Random;
                MinIterations = randomLoopConfig.MinIterations;
                MaxIterations = randomLoopConfig.MaxIterations;
                break;
            }
        }
    }

    public ProcessConfigViewModel Copy()
    {
        ProcessConfigViewModel copy = new()
        {
            LoopType = LoopType,
            Name = Name,
            Priority = Priority,
            Iterations = Iterations,
            MinIterations = MinIterations,
            MaxIterations = MaxIterations,
            Operations = new ObservableCollection<OperationConfigViewModel>(Operations.Select(op => op.Copy()))
        };
        return copy;
    }

    public bool Validate(out IEnumerable<string> errors)
    {
        List<string> list = [];

        if (string.IsNullOrWhiteSpace(Name))
            list.Add("O nome do processo não pode estar vazio.");

        if (LoopType == LoopType.None)
            list.Add("Tipo de loop deve ser definido.");
        else if (LoopType == LoopType.Finite && Iterations == 0)
            list.Add("O número de iterações não pode ser zero.");
        else if (LoopType == LoopType.Random && (MinIterations == 0 || MaxIterations < MinIterations))
            list.Add("O número de iterações aleatórias não pode ser zero ou o máximo deve ser menor que o mínimo.");

        if (Operations.Count == 0)
            list.Add("O processo deve ter pelo menos uma operação.");

        for (int idx = 0; idx < Operations.Count; idx++)
        {
            OperationConfigViewModel operation = Operations.ElementAt(idx);
            if (!operation.Validate(out IEnumerable<string> opErrors))
            {
                foreach (string e in opErrors)
                    list.Add($"#{idx} [{operation.Summary}]: {e}");
            }
        }

        errors = list;
        return list.Count == 0;
    }

    public bool Equals(ProcessConfigViewModel other)
    {
        if (ReferenceEquals(other, this)) return true;
        if (other is null) return false;
        if (Name != other.Name) return false;
        if (Priority != other.Priority) return false;
        if (LoopType != other.LoopType) return false;
        if (Operations.Count != other.Operations.Count) return false;
        if (LoopType == other.LoopType) return false;
        if (Iterations != other.Iterations) return false;
        if (MinIterations != other.MinIterations) return false;
        if (MaxIterations != other.MaxIterations) return false;

        for (int i = 0; i < Operations.Count; i++)
        {
            if (!Operations[i].Equals(other.Operations[i]))
                return false;
        }

        return true;
    }

    private void AddOperation()
    {
        OperationConfigViewModel operation = new();
        Operations.Add(operation);
        SelectedOperation = operation;
    }

    private void RemoveOperation(OperationConfigViewModel operation)
    {
        if (operation is not null && Operations.Contains(operation))
            Operations.Remove(operation);
    }
}