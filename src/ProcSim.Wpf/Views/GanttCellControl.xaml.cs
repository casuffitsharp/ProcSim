using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ProcSim.Wpf.Views;

public partial class GanttCellControl : UserControl
{
    // Propriedade de dependência para receber o Brush que será animado
    public static readonly DependencyProperty BrushProperty = DependencyProperty.Register("Brush", typeof(Brush), typeof(GanttCellControl), new PropertyMetadata(null));

    public Brush Brush
    {
        get => (Brush)GetValue(BrushProperty);
        set => SetValue(BrushProperty, value);
    }

    public GanttCellControl()
    {
        InitializeComponent();
        Loaded += GanttCellControl_Loaded;
    }

    private void GanttCellControl_Loaded(object sender, RoutedEventArgs e)
    {
        // Anima o ScaleX do retângulo de 0 para 1 em 0,5 segundos
        var animation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromSeconds(0.5),
            FillBehavior = FillBehavior.HoldEnd
        };

        ScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
    }
}
