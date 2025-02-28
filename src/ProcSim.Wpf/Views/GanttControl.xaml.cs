using ProcSim.Wpf.Helpers;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ProcSim.Wpf.Views;

public partial class GanttControl : UserControl
{
    public GanttControl()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(GanttControl), new PropertyMetadata(null));
    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty TimeUnitsProperty = DependencyProperty.Register("TimeUnits", typeof(int), typeof(GanttControl), new PropertyMetadata(0, OnTimeUnitsChanged));
    public int TimeUnits
    {
        get => (int)GetValue(TimeUnitsProperty);
        set => SetValue(TimeUnitsProperty, value);
    }

    private static void OnTimeUnitsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GanttControl control)
        {
            control.UpdateColumns((int)e.NewValue);
        }
    }

    public static readonly DependencyProperty ColorConverterProperty = DependencyProperty.Register("ColorConverter", typeof(IMultiValueConverter), typeof(GanttControl), new PropertyMetadata(null));
    public System.Windows.Data.IMultiValueConverter ColorConverter
    {
        get => (System.Windows.Data.IMultiValueConverter)GetValue(ColorConverterProperty);
        set => SetValue(ColorConverterProperty, value);
    }

    public static readonly DependencyProperty AnimationDurationProperty = DependencyProperty.Register("AnimationDuration", propertyType: typeof(System.TimeSpan), typeof(GanttControl), new PropertyMetadata(System.TimeSpan.FromSeconds(0.5)));
    public System.TimeSpan AnimationDuration
    {
        get => (System.TimeSpan)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    public void UpdateColumns(int totalTimeUnits)
    {
        if (totalTimeUnits < dataGridGantt.Columns.Count)
            return;

        for (int t = dataGridGantt.Columns.Count - 1; t < totalTimeUnits; t++)
        {
            DataGridTemplateColumn col = new()
            {
                Header = t.ToString(), // Cabeçalho com o índice
                HeaderStyle = (Style)FindResource("CenteredHeaderStyle"),
                CellTemplate = (DataTemplate)FindResource("GanttCellAnimatedTemplate")
            };

            // Define a attached property para armazenar o índice da coluna
            DataGridColumnExtensions.SetColumnIndex(col, t);
            dataGridGantt.Columns.Add(col);
        }
    }

    public void Reset()
    {
        DataGridColumn firstColumn = dataGridGantt.Columns[0];
        dataGridGantt.Columns.Clear();
        dataGridGantt.Columns.Add(firstColumn);
    }
}
