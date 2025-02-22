using System.ComponentModel;
using ProcSim.Core.Entities;
using ProcSim.Core.Enums;

namespace ProcSim.WPF.ViewModels;

public class ProcessViewModel(Process process) : INotifyPropertyChanged
{
    public Process Model => process;

    public int Id => process.Id;

    public string Name
    {
        get => process.Name;
        set
        {
            if (process.Name != value)
            {
                process.Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    public int ExecutionTime
    {
        get => process.ExecutionTime;
        set
        {
            if (process.ExecutionTime != value)
            {
                process.ExecutionTime = value;
                OnPropertyChanged(nameof(ExecutionTime));
            }
        }
    }

    public int RemainingTime
    {
        get => process.RemainingTime;
        set
        {
            if (process.RemainingTime != value)
            {
                process.RemainingTime = value;
                OnPropertyChanged(nameof(RemainingTime));
            }
        }
    }

    public ProcessState State
    {
        get => process.State;
        set
        {
            if (process.State != value)
            {
                process.State = value;
                OnPropertyChanged(nameof(State));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected virtual void OnPropertyChanged(string propertyName) => NotifyPropertyChanged(propertyName);
}
