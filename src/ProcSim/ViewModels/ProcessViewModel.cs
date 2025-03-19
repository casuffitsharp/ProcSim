using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcSim.Core.Enums;
using ProcSim.Core.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ProcSim.ViewModels;

public partial class ProcessViewModel : ObservableObject
{
    public ProcessViewModel(Process process)
    {
        Model = process;
        Name = process.Name;
        Operations = [.. process.Operations.Select(o => new OperationViewModel(o))];
        foreach (var op in Operations)
            SubscribeOperation(op);

        AddOperationCommand = new RelayCommand(AddOperation);
        RemoveOperationCommand = new RelayCommand<OperationViewModel>(RemoveOperation);
        Operations.CollectionChanged += Operations_CollectionChanged;
    }

    public ProcessViewModel(int id) : this(new Process(id, string.Empty, [])) { }

    [ObservableProperty]
    public partial OperationViewModel SelectedOperation { get; set; }

    public IRelayCommand AddOperationCommand { get; }
    public IRelayCommand<OperationViewModel> RemoveOperationCommand { get; }

    public Process Model { get; }

    public int Id => Model.Id;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    public partial string Name { get; set; }

    public ProcessState State => Model.State;

    public ObservableCollection<OperationViewModel> Operations { get; private set; }

    public bool IsValid => !string.IsNullOrWhiteSpace(Name) && Operations?.Count > 0 && Operations.All(o => o.IsValid) == true;

    public void UpdateFromModel()
    {
        OnPropertyChanged(nameof(State));
    }

    public ProcessViewModel Commit()
    {
        return new(new Process(Model.Id, Name, [.. Operations.Select(o => o.Commit().Model)]));
    }

    public void Reset()
    {
        Name = Model.Name;
        Operations = [.. Model.Operations.Select(o => new OperationViewModel(o))];
        SelectedOperation = null;
    }

    public bool HasChanges
    {
        get
        {
            if (Name != Model.Name)
                return true;

            if (Operations.Count != Model.Operations.Count)
                return true;

            for (int i = 0; i < Operations.Count; i++)
            {
                if (Operations[i].Model != Model.Operations[i])
                    return true;

                if (Operations[i].HasChanges)
                    return true;
            }

            return false;
        }
    }

    private void Operations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (OperationViewModel newOp in e.NewItems)
                SubscribeOperation(newOp);
        }

        if (e.OldItems != null)
        {
            foreach (OperationViewModel oldOp in e.OldItems)
                UnsubscribeOperation(oldOp);
        }

        OnPropertyChanged(nameof(IsValid));
        OnPropertyChanged(nameof(HasChanges));
    }

    private void Operation_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(OperationViewModel.IsValid) or nameof(OperationViewModel.Duration))
        {
            OnPropertyChanged(nameof(IsValid));
            OnPropertyChanged(nameof(HasChanges));
        }
    }

    private void SubscribeOperation(OperationViewModel op)
    {
        op.PropertyChanged += Operation_PropertyChanged;
    }

    private void UnsubscribeOperation(OperationViewModel op)
    {
        op.PropertyChanged -= Operation_PropertyChanged;
    }

    private void AddOperation()
    {
        OperationViewModel operation = new();
        Operations.Add(operation);
        SelectedOperation = operation;
    }

    private void RemoveOperation(OperationViewModel operation)
    {
        if (operation is not null && Operations.Contains(operation))
            Operations.Remove(operation);
    }
}
