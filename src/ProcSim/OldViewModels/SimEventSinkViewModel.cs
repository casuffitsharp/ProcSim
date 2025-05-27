using CommunityToolkit.Mvvm.ComponentModel;
using ProcSim.Core.Logging;
using System.Collections.ObjectModel;
using System.Windows;

namespace ProcSim.ViewModels;

public class SimEventSinkViewModel : ObservableObject
{
    public ObservableCollection<SimEvent> Events { get; } = [];

    public SimEventSinkViewModel(IStructuredLogger logger)
    {
        logger.OnLog += OnLogReceived;
    }

    private void OnLogReceived(SimEvent simEvent)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Events.Add(simEvent);
        });
    }
}
