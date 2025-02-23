using System.Collections.ObjectModel;
using ProcSim.Core.Enums;
using ProcSim.Core.Factories;
using ProcSim.Core.Scheduling;

namespace ProcSim.Wpf.ViewModels;

public class SimulationSettingsViewModel : ViewModelBase
{
    public ObservableCollection<SchedulingAlgorithmType> Algorithms { get; } = [.. Enum.GetValues<SchedulingAlgorithmType>()];

    private SchedulingAlgorithmType _selectedAlgorithm = SchedulingAlgorithmType.Fcfs;
    private ISchedulingAlgorithm _selectedAlgorithmInstance;

    public SimulationSettingsViewModel()
    {
        _selectedAlgorithmInstance = SchedulingAlgorithmFactory.Create(_selectedAlgorithm);
    }

    public SchedulingAlgorithmType SelectedAlgorithm
    {
        get => _selectedAlgorithm;
        set
        {
            if (_selectedAlgorithm != value)
            {
                _selectedAlgorithm = value;
                _selectedAlgorithmInstance = SchedulingAlgorithmFactory.Create(value);
                
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPreemptive));
                OnPropertyChanged(nameof(SelectedAlgorithmInstance));
            }
        }
    }

    public ISchedulingAlgorithm SelectedAlgorithmInstance => _selectedAlgorithmInstance;

    public bool IsPreemptive => _selectedAlgorithmInstance.IsPreemptive;

    public int Quantum { get; set; } = 1;
}
