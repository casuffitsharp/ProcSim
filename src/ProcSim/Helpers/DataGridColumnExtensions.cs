using System.Windows;
using System.Windows.Controls;

namespace ProcSim.Wpf.Helpers;

public static class DataGridColumnExtensions
{
    public static readonly DependencyProperty ColumnIndexProperty =
        DependencyProperty.RegisterAttached(
            "ColumnIndex",
            typeof(int),
            typeof(DataGridColumnExtensions),
            new PropertyMetadata(0));

    public static void SetColumnIndex(DataGridColumn element, int value)
    {
        element.SetValue(ColumnIndexProperty, value);
    }

    public static int GetColumnIndex(DataGridColumn element)
    {
        return (int)element.GetValue(ColumnIndexProperty);
    }
}
