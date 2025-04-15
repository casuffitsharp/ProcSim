using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProcSim.Views;

public partial class ProcessListControl : UserControl
{
    public ProcessListControl()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(string), typeof(ProcessListControl), new PropertyMetadata(string.Empty));
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(ProcessListControl), new PropertyMetadata(null));
    public static readonly DependencyProperty CardBackgroundProperty = DependencyProperty.Register(nameof(CardBackground), typeof(Brush), typeof(ProcessListControl), new PropertyMetadata(Brushes.Transparent));

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

    public Brush CardBackground
    {
        get => (Brush)GetValue(CardBackgroundProperty);
        set => SetValue(CardBackgroundProperty, value);
    }
}
