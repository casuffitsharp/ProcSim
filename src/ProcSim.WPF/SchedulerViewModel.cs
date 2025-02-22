using ProcSim.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ProcSim.WPF;

public class SchedulerViewModel : INotifyPropertyChanged
{
    private readonly Scheduler _scheduler = new();
    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<Process> Processes { get; } = [];

    public void AddProcess(Process process)
    {
        Processes.Add(process);
        _scheduler.AddProcess(process);
    }

    public void RunScheduling()
    {
        _scheduler.Run();
        OnPropertyChanged(nameof(Processes));
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
