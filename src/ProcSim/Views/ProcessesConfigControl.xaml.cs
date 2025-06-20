using System.Windows;
using System.Windows.Controls;

namespace ProcSim.Views;

public partial class ProcessesConfigControl : UserControl
{
    public ProcessesConfigControl()
    {
        InitializeComponent();
    }

    private void OnSelectedProcessChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        (sender as Grid).IsEnabled = e.NewValue != null;
    }
}
