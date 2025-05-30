﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ProcSim.ViewModels;

public class SimulationControlViewModel : ObservableObject
{
    public IRelayCommand StartCommand { get; }
    public IRelayCommand PauseCommand { get; }
    public IRelayCommand ResetCommand { get; }
    public IRelayCommand AddProcessCommand { get; }

    //public SimulationControlViewModel(SimulationController controller, ObservableCollection<ProcessViewModel> processes)
    //{
    //    StartCommand = new RelayCommand(controller.Start);
    //    PauseCommand = new RelayCommand(controller.Pause);
    //    ResetCommand = new RelayCommand(controller.Reset);
    //    AddProcessCommand = new RelayCommand(() =>
    //    {
    //        // cria Rn no processo config e já injeta via controller.AddProcess
    //    });
    //}
}