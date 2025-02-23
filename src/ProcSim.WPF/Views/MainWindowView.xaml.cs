using ProcSim.Wpf.ViewModels;
using System.Windows;

namespace ProcSim.Wpf.Views;

public partial class MainWindowView : Window
{
    private readonly MainViewModel _schedulerViewModel;

    public MainWindowView()
    {
        InitializeComponent();

        _schedulerViewModel = new MainViewModel();
        DataContext = _schedulerViewModel;
    }

    private async void RunScheduler_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            await viewModel.RunSchedulingAsync();
        }
    }
}
