using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Core.Enums;
using ProcSim.Core.Models;
using System.Collections.ObjectModel;

namespace ProcSim.Wpf.ViewModels;

public class ProcessViewModel(Process process) : ObservableObject
{
    private readonly Process _process = process;

    public Process Model => _process;

    public int Id => _process.Id;
    public string Name => _process.Name;
    public int ExecutionTime => _process.ExecutionTime;
    public int IoTime => _process.IoTime;
    public int RemainingTime => _process.RemainingTime;
    public ProcessState State => _process.State;
    public ProcessType Type => _process.Type;

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
