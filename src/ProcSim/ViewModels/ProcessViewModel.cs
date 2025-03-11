using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Core.Enums;
using ProcSim.Core.Models;
using System.Collections.ObjectModel;

namespace ProcSim.ViewModels;

public class ProcessViewModel(Process process) : ObservableObject
{
    public Process Model { get; } = process;

    public int Id => Model.Id;
    public string Name => Model.Name;
    public ProcessState State => Model.State;

    public void UpdateFromModel()
    {
        OnPropertyChanged(nameof(State));
    }
}
