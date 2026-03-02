using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProcSim.Views;

public partial class ProcessesConfigControl : UserControl
{
    public ProcessesConfigControl()
    {
        InitializeComponent();
    }

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
    private void OnSelectedProcessChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is Grid grid)
            grid.IsEnabled = e.NewValue != null;
    }

    private void OnProcessListPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (FindAncestor<DataGridRow>(e.OriginalSource as DependencyObject) is null)
            ProcessesDataGrid.SelectedItem = null;
    }
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static

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
