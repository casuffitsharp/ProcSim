using System.Collections.ObjectModel;
using System.Windows.Input;
using ProcSim.Core.Enums;
using ProcSim.Core.Models;
using ProcSim.Wpf.Commands;

namespace ProcSim.Wpf.ViewModels;

public class ProcessRegistrationViewModel : ViewModelBase
{
    private int _nextProcessId = 1;

    public ProcessRegistrationViewModel(ObservableCollection<ProcessViewModel> processes)
    {
        Processes = processes;
        AddProcessCommand = new RelayCommand(AddProcess, CanAddProcess);
        ResetNewProcess();
    }

    public ObservableCollection<ProcessViewModel> Processes { get; }

    private string _name = string.Empty;
    private int _executionTime;
    private int _ioTime;
    private bool _isCpuBound = true;

    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }

    public int ExecutionTime
    {
        get => _executionTime;
        set
        {
            if (_executionTime != value)
            {
                _executionTime = value;
                OnPropertyChanged();
            }
        }
    }

    public int IoTime
    {
        get => _ioTime;
        set
        {
            if (_ioTime != value)
            {
                _ioTime = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsCpuBound
    {
        get => _isCpuBound;
        set
        {
            if (_isCpuBound != value)
            {
                _isCpuBound = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsIoBound));
            }
        }
    }

    public bool IsIoBound
    {
        get => !_isCpuBound;
        set
        {
            if (IsIoBound != value)
            {
                IsCpuBound = !value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand AddProcessCommand { get; }

    private void AddProcess()
    {
        var process = new Process(
            id: _nextProcessId++,
            name: Name,
            executionTime: ExecutionTime,
            ioTime: IsIoBound ? IoTime : 0,
            type: IsCpuBound ? ProcessType.CpuBound : ProcessType.IoBound
        );

        Processes.Add(new ProcessViewModel(process));
        ResetNewProcess();
    }

    private bool CanAddProcess() =>
        !string.IsNullOrWhiteSpace(Name) && ExecutionTime > 0;

    private void ResetNewProcess()
    {
        Name = string.Empty;
        ExecutionTime = 0;
        IoTime = 0;
        IsCpuBound = true; // Reseta para CPU-bound por padrão
    }
}
