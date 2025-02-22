using ProcSim.Core;
using ProcSim.Core.Entities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;

namespace ProcSim.WPF.ViewModels;

public class SchedulerViewModel : INotifyPropertyChanged
{
    private readonly Scheduler _scheduler = new();

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<ProcessViewModel> Processes { get; } = [];

    public SchedulerViewModel()
    {
        _scheduler.ProcessUpdated += OnProcessUpdated;
    }

    public void AddProcess(ProcessViewModel processViewModel)
    {
        Processes.Add(processViewModel);
        _scheduler.AddProcess(processViewModel.Model);
    }

    public async Task RunSchedulingAsync()
    {
        await Task.Run(_scheduler.Run);
        // If needed, signal that the collection has been updated
        //OnPropertyChanged(nameof(Processes));
    }

    private void OnProcessUpdated(Process updatedProcess)
    {
        var vm = Processes.FirstOrDefault(p => p.Model == updatedProcess);
        if (vm is null)
            return;

        // Force update by raising PropertyChanged notifications for properties that may change
        vm.NotifyPropertyChanged(nameof(vm.RemainingTime));
        vm.NotifyPropertyChanged(nameof(vm.State));
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
