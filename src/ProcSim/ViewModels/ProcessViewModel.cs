using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Core.Enums;
using ProcSim.Core.Models;
using System.Collections.ObjectModel;

namespace ProcSim.ViewModels;

public partial class ProcessViewModel(Process process) : ObservableObject
{
    public Process Model { get; } = process;

    public ProcessViewModel(int id) : this(new Process(id, string.Empty, [])) { }

    public int Id => Model.Id;

    [ObservableProperty]
    public partial string Name { get; set; } = process.Name;

    public ProcessState State => Model.State;

    public ObservableCollection<OperationViewModel> Operations { get; private set; } = [.. process.Operations.Select(o => new OperationViewModel(o))];

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
    }
}
