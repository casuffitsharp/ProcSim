using System.Windows;
using System.Windows.Controls;

namespace ProcSim.Views;

public partial class OperationPanelControl : UserControl
{
    public OperationPanelControl()
    {
        InitializeComponent();
        IsEnabled = DataContext != null;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        IsEnabled = e.NewValue != null;
    }
}