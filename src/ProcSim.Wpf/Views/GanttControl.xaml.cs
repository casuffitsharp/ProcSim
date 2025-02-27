using System.Collections;
using System.Windows;
using System.Windows.Controls;
using ProcSim.Wpf.Helpers;

namespace ProcSim.Wpf.Views;

public partial class GanttControl : UserControl
{
    private int _currentDynamicColumns = 0;

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

    public static readonly DependencyProperty ColorConverterProperty = DependencyProperty.Register("ColorConverter", typeof(System.Windows.Data.IMultiValueConverter), typeof(GanttControl), new PropertyMetadata(null));
    public System.Windows.Data.IMultiValueConverter ColorConverter
    {
        get => (System.Windows.Data.IMultiValueConverter)GetValue(ColorConverterProperty);
        set => SetValue(ColorConverterProperty, value);
    }

    public static readonly DependencyProperty AnimationDurationProperty = DependencyProperty.Register("AnimationDuration", typeof(System.TimeSpan), typeof(GanttControl), new PropertyMetadata(System.TimeSpan.FromSeconds(0.5)));
    public System.TimeSpan AnimationDuration
    {
        get => (System.TimeSpan)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    // Cria as colunas dinâmicas com base no número de TimeUnits
    public void UpdateColumns(int totalTimeUnits)
    {
        if (totalTimeUnits <= _currentDynamicColumns)
            return;

        for (int t = _currentDynamicColumns; t < totalTimeUnits; t++)
        {
            var col = new DataGridTemplateColumn
            {
                Header = t.ToString(), // Cabeçalho com o índice
                HeaderStyle = (Style)FindResource("CenteredHeaderStyle"),
                CellTemplate = (DataTemplate)FindResource("GanttCellAnimatedTemplate")
            };

            // Define a attached property para armazenar o índice da coluna
            DataGridColumnExtensions.SetColumnIndex(col, t);
            dataGridGantt.Columns.Add(col);
        }

        _currentDynamicColumns = totalTimeUnits;
    }
}
