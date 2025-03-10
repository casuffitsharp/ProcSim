﻿using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Core.Enums;
using ProcSim.Core.Factories;
using ProcSim.Core.Scheduling.Algorithms;

namespace ProcSim.ViewModels;

public partial class SimulationSettingsViewModel : ObservableObject
{
    public List<SchedulingAlgorithmType> Algorithms { get; } = [.. Enum.GetValues<SchedulingAlgorithmType>()];

    public SimulationSettingsViewModel()
    {
        SelectedAlgorithmInstance = SchedulingAlgorithmFactory.Create(SelectedAlgorithm);
        Quantum = 1;
        CanChangeAlgorithm = true;
    }

    public SchedulingAlgorithmType SelectedAlgorithm
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                SelectedAlgorithmInstance = SchedulingAlgorithmFactory.Create(value);

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPreemptive));
                OnPropertyChanged(nameof(SelectedAlgorithmInstance));
            }
        }
    } = SchedulingAlgorithmType.Fcfs;

    public ISchedulingAlgorithm SelectedAlgorithmInstance { get; private set; }

    public bool IsPreemptive => SelectedAlgorithmInstance is IPreemptiveAlgorithm;

    public int Quantum
    {
        get;
        set
        {
            if (field != value && value > 0)
            {
                field = value;
                OnPropertyChanged();
                UpdateSchedulerQuantum();
            }
        }
    }

    [ObservableProperty]
    public partial bool CanChangeAlgorithm { get; set; }

    private void UpdateSchedulerQuantum()
    {
        if (SelectedAlgorithmInstance is IPreemptiveAlgorithm preemptiveAlgorithm)
        {
            preemptiveAlgorithm.Quantum = Quantum;
        }
    }
}
