using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Core.Enums;
using ProcSim.Core.Models;
using System.Collections.ObjectModel;

namespace ProcSim.Wpf.ViewModels;

public class ProcessViewModel(Process process) : ObservableObject
{
    public Process Model { get; } = process;

    public int Id => Model.Id;
    public string Name => Model.Name;
    public int ExecutionTime => Model.ExecutionTime;
    public int IoTime => Model.IoTime;
    public int RemainingTime => Model.RemainingTime;
    public ProcessState State => Model.State;
    public ProcessType Type => Model.Type;

    // Histórico dos estados ao longo do tempo.
    public ObservableCollection<ProcessState> StateHistory { get; } = [];

    public void Tick()
    {
        StateHistory.Add(State);
        OnPropertyChanged(nameof(StateHistory));
    }

    public void UpdateFromModel()
    {
        OnPropertyChanged(nameof(RemainingTime));
        OnPropertyChanged(nameof(State));
    }
}
