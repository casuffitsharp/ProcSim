using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcSim.Core.Enums;
using ProcSim.Core.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ProcSim.ViewModels;

public partial class ProcessViewModel : ObservableObject
{
    public ProcessViewModel(Process process)
    {
        Model = process;
        Name = process.Name;
        Operations = [.. process.Operations.Select(o => new OperationViewModel(o))];
        Operations.CollectionChanged += Operations_CollectionChanged;
        foreach (var op in Operations)
            op.PropertyChanged += (s, e) => OnPropertyChanged(nameof(HasChanges));

        AddOperationCommand = new RelayCommand(AddOperation);
        RemoveOperationCommand = new RelayCommand<OperationViewModel>(RemoveOperation);
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

    public bool IsValid => !string.IsNullOrWhiteSpace(Name) && Operations?.Any() == true;

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
        OnPropertyChanged(nameof(IsValid));
        OnPropertyChanged(nameof(HasChanges));
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
