using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ProcSim.Wpf.Helpers;

public static class SmoothBackgroundTransitionBehavior
{
    public static readonly DependencyProperty EnableSmoothTransitionProperty = DependencyProperty.RegisterAttached("EnableSmoothTransition", typeof(bool), typeof(SmoothBackgroundTransitionBehavior), new PropertyMetadata(false, OnEnableSmoothTransitionChanged));
    private static readonly DependencyProperty PreviousBrushProperty = DependencyProperty.RegisterAttached("PreviousBrush", typeof(Brush), typeof(SmoothBackgroundTransitionBehavior), new PropertyMetadata(null));
    public static readonly DependencyProperty IsAnimatingProperty = DependencyProperty.RegisterAttached("IsAnimating", typeof(bool), typeof(SmoothBackgroundTransitionBehavior), new PropertyMetadata(false));

    public static bool GetEnableSmoothTransition(DependencyObject obj) => (bool)obj.GetValue(EnableSmoothTransitionProperty);
    public static void SetEnableSmoothTransition(DependencyObject obj, bool value) => obj.SetValue(EnableSmoothTransitionProperty, value);

    private static Brush GetPreviousBrush(DependencyObject obj) => (Brush)obj.GetValue(PreviousBrushProperty);
    private static void SetPreviousBrush(DependencyObject obj, Brush value) => obj.SetValue(PreviousBrushProperty, value);

    public static bool GetIsAnimating(DependencyObject obj) => (bool)obj.GetValue(IsAnimatingProperty);
    public static void SetIsAnimating(DependencyObject obj, bool value) => obj.SetValue(IsAnimatingProperty, value);

    private static void OnEnableSmoothTransitionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGridCell cell)
            return;

        DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(Control.BackgroundProperty, typeof(DataGridCell));
        if ((bool)e.NewValue)
        {
            dpd.AddValueChanged(cell, OnBackgroundChanged);
        }
        else
        {
            dpd.RemoveValueChanged(cell, OnBackgroundChanged);
        }
    }

    private static void OnBackgroundChanged(object sender, EventArgs e)
    {
        if (sender is not DataGridCell cell)
            return;

        // Se já estiver animando, não entra novamente.
        if (GetIsAnimating(cell))
            return;

        // Verifica se o novo Background é um SolidColorBrush
        if (cell.Background is not SolidColorBrush newBrush)
            return;

        // Define a cor inicial como a última cor animada, se houver; caso contrário, usa a cor atual
        Color startColor = GetPreviousBrush(cell) is SolidColorBrush prevBrush ? prevBrush.Color : newBrush.Color;
        Color endColor = newBrush.Color;

        // Cria um novo brush para animar
        var animBrush = new SolidColorBrush(startColor);

        // Cria a animação para a propriedade Color
        var animation = new ColorAnimation(endColor, TimeSpan.FromMilliseconds(300))
        {
            EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseInOut }
        };

        // Sinaliza que a célula está animando
        SetIsAnimating(cell, true);

        // Quando a animação terminar, desativa a flag
        animation.Completed += (s, args) =>
        {
            SetIsAnimating(cell, false);
            // Atualiza o PreviousBrush para o brush animado final
            SetPreviousBrush(cell, animBrush);
        };

        // Inicia a animação
        animBrush.BeginAnimation(SolidColorBrush.ColorProperty, animation, HandoffBehavior.Compose);

        // Define o Background da célula com o brush animado
        cell.Background = animBrush;
    }
}
