using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProcSim.Views;

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
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

    private void OnProcessListPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (FindAncestor<DataGridRow>(e.OriginalSource as DependencyObject) is null)
            ProcessesDataGrid.SelectedItem = null;
    }

    private static T FindAncestor<T>(DependencyObject element) where T : DependencyObject
    {
        while (element is not null)
        {
            if (element is T target) return target;
            element = VisualTreeHelper.GetParent(element);
        }
        return null;
    }
}
