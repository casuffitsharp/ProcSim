using System.Collections.ObjectModel;
using System.Windows.Input;
using ProcSim.Core.Enums;
using ProcSim.Core.Models;
using ProcSim.WPF.Commands;

namespace ProcSim.WPF.ViewModels;

public class ProcessRegistrationViewModel : ViewModelBase
{
    public ProcessRegistrationViewModel(ObservableCollection<Process> processes)
    {
        Processes = processes;
        AddProcessCommand = new RelayCommand(AddProcess, CanAddProcess);
    }

    public ObservableCollection<Process> Processes { get; }
    public Process NewProcess { get; private set; } = new(0, "", 0, 0, ProcessType.CpuBound);
    public ICommand AddProcessCommand { get; }

    private void AddProcess()
    {
        var process = new Process(
            id: Processes.Count + 1,
            name: NewProcess.Name,
            executionTime: NewProcess.ExecutionTime,
            ioTime: NewProcess.IoTime,
            type: NewProcess.Type
        );

        Processes.Add(process);

        // Reset NewProcess after adding
        NewProcess = new Process(0, "", 0, 0, ProcessType.CpuBound);
        OnPropertyChanged(nameof(NewProcess));
    }

    private bool CanAddProcess() => !string.IsNullOrWhiteSpace(NewProcess.Name) && NewProcess.ExecutionTime > 0;
}
