using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcSim.Core.Models;
using ProcSim.Core.Models.Operations;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ProcSim.ViewModels;

public partial class ProcessRegistrationViewModel : ObservableObject
{
    private int _nextProcessId = 1;

    public ProcessRegistrationViewModel(ObservableCollection<ProcessViewModel> processes)
    {
        Processes = processes;
        AddProcessCommand = new RelayCommand(AddProcess, CanAddProcess);
        ResetNewProcess();
    }

    public ObservableCollection<ProcessViewModel> Processes { get; }

    [ObservableProperty]
    public partial string Name { get; set; }

    public IRelayCommand AddProcessCommand { get; }

    private void AddProcess()
    {
        List<IOperation> operations = [];
        Process process = new(
            id: _nextProcessId++,
            name: Name,
            operations
        );

        Processes.Add(new ProcessViewModel(process));
        ResetNewProcess();
    }

    private bool CanAddProcess()
    {
        return !string.IsNullOrWhiteSpace(Name);
    }

    private void ResetNewProcess()
    {
        Name = string.Empty;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName is nameof(Name))
        {
            AddProcessCommand.NotifyCanExecuteChanged();
        }
    }
}
