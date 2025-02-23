using System.Windows;
using ProcSim.WPF.ViewModels;

namespace ProcSim.WPF.Views;

public partial class MainWindowView : Window
{
    private readonly SchedulerViewModel _schedulerViewModel;

    public MainWindowView()
    {
        InitializeComponent();

        _schedulerViewModel = new SchedulerViewModel();
        DataContext = _schedulerViewModel;
    }

    private async void RunScheduler_Click(object sender, RoutedEventArgs e)
    {
        await _schedulerViewModel.RunSchedulingAsync();
    }
}
