using ProcSim.Core.Entities;
using ProcSim.WPF.ViewModels;
using System.Windows;

namespace ProcSim.WPF;

public partial class MainWindow : Window
{
    private readonly SchedulerViewModel _viewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
        // Example data
        _viewModel.AddProcess(new ProcessViewModel(new Process
        {
            Id = 1,
            Name = "Process1",
            RemainingTime = 5
        }));

        _viewModel.AddProcess(new ProcessViewModel(new Process
        {
            Id = 2,
            Name = "Process2",
            RemainingTime = 2
        }));
    }

    private void RunScheduler_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.RunSchedulingAsync();
    }
}
