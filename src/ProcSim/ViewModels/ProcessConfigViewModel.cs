using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcSim.Core.Configuration;
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

public partial class ProcessConfigViewModel : ObservableObject, IEquatable<ProcessConfigViewModel>
{
    public ProcessConfigViewModel()
    {
        AddOperationCommand = new RelayCommand(AddOperation);
        RemoveOperationCommand = new RelayCommand<OperationConfigViewModel>(RemoveOperation);
        Operations.CollectionChanged += Operations_CollectionChanged;
    }

    public ProcessConfigViewModel(ProcessConfigModel model) : this()
    {
        UpdateFromModel(model);
    }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial uint Priority { get; set; }

    [ObservableProperty]
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

    public bool IsValid
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Name)) return false;

            if (LoopType == LoopType.Finite && Iterations == 0) return false;
            if (LoopType == LoopType.Random && (MinIterations == 0 || MaxIterations < MinIterations)) return false;

            if (Operations.Count == 0) return false;
            if (Operations.Any(op => !op.IsValid)) return false;

            return true;
        }
    }

    public override bool Equals(object obj)
    {
        return obj is ProcessConfigViewModel other && Equals(other);
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

    private void Operations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        //OnPropertyChanged(nameof(IsValid));
        //OnPropertyChanged(nameof(HasChanges));
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