using ProcSim.Core.Enums;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ProcSim.Views;

public partial class GanttCellControl : UserControl
{
    public GanttCellControl()
    {
        InitializeComponent();
        Loaded += GanttCellControl_Loaded;
    }

    public static readonly DependencyProperty StateHistoryProperty = DependencyProperty.Register("StateHistory", typeof(IList<ProcessState>), typeof(GanttCellControl), new PropertyMetadata(null, OnStateHistoryOrColumnIndexChanged));
    public IList<ProcessState> StateHistory
    {
        get => (IList<ProcessState>)GetValue(StateHistoryProperty);
        set => SetValue(StateHistoryProperty, value);
    }

    public static readonly DependencyProperty ColumnIndexProperty = DependencyProperty.Register("ColumnIndex", typeof(int), typeof(GanttCellControl), new PropertyMetadata(0, OnStateHistoryOrColumnIndexChanged));
    public int ColumnIndex
    {
        get => (int)GetValue(ColumnIndexProperty);
        set => SetValue(ColumnIndexProperty, value);
    }

    public static readonly DependencyProperty ColorConverterProperty = DependencyProperty.Register("ColorConverter", typeof(IMultiValueConverter), typeof(GanttCellControl), new PropertyMetadata(null, OnStateHistoryOrColumnIndexChanged));
    public IMultiValueConverter ColorConverter
    {
        get => (IMultiValueConverter)GetValue(ColorConverterProperty);
        set => SetValue(ColorConverterProperty, value);
    }

    public static readonly DependencyProperty AnimationDurationProperty = DependencyProperty.Register("AnimationDuration", typeof(TimeSpan), typeof(GanttCellControl), new PropertyMetadata(TimeSpan.FromSeconds(0.5)));
    public TimeSpan AnimationDuration
    {
        get => (TimeSpan)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    public static readonly DependencyProperty BrushProperty = DependencyProperty.Register("Brush", typeof(Brush), typeof(GanttCellControl), new PropertyMetadata(Brushes.Transparent));
    public Brush Brush
    {
        get => (Brush)GetValue(BrushProperty);
        set => SetValue(BrushProperty, value);
    }

    private static void OnStateHistoryOrColumnIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        GanttCellControl control = (GanttCellControl)d;
        control.UpdateBrush();
    }

    private void UpdateBrush()
    {
        if (StateHistory == null || ColumnIndex < 0 || ColumnIndex >= StateHistory.Count)
        {
            Brush = Brushes.Transparent;
            return;
        }

        if (ColorConverter != null)
        {
            object result = ColorConverter.Convert([StateHistory, ColumnIndex], typeof(Brush), null, CultureInfo.CurrentCulture);
            Brush = result as Brush ?? Brushes.Transparent;
            return;
        }
    }

    private void GanttCellControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (AnimatedRect.RenderTransform is not ScaleTransform st)
        {
            st = new ScaleTransform(0, 1);
            AnimatedRect.RenderTransform = st;
            AnimatedRect.RenderTransformOrigin = new Point(0, 0.5);
        }

        DoubleAnimation anim = new()
        {
            From = 0,
            To = 1,
            Duration = AnimationDuration,
            FillBehavior = FillBehavior.HoldEnd
        };

        st.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
    }
}
