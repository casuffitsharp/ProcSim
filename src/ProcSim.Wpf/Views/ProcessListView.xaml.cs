using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace ProcSim.Wpf.Views;

public partial class ProcessListView : UserControl
{
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(string), typeof(ProcessListView), new PropertyMetadata(string.Empty));
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(ProcessListView), new PropertyMetadata(null));

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public ProcessListView()
    {
        InitializeComponent();
    }
}
