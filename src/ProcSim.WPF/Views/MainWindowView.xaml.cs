using MaterialDesignThemes.Wpf;
using ProcSim.Wpf.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProcSim.Wpf.Views;

public partial class MainWindowView : Window
{
    private readonly MainViewModel _mainViewModel;

    public MainWindowView()
    {
        InitializeComponent();

        _mainViewModel = new MainViewModel();
        DataContext = _mainViewModel;
    }

    private static void ModifyTheme(bool isDarkTheme)
    {
        PaletteHelper paletteHelper = new();
        Theme theme = paletteHelper.GetTheme();

        theme.SetBaseTheme(isDarkTheme ? BaseTheme.Dark : BaseTheme.Light);
        paletteHelper.SetTheme(theme);
    }

    private void MenuDarkModeButton_Click(object sender, RoutedEventArgs e)
    {
        ModifyTheme(DarkModeToggleButton.IsChecked == true);
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        ganttControl.Reset();
    }

    private void Slider_DragCompleted(object sender, object e)
    {
        if (sender is Slider slider)
        {
            var binding = slider.GetBindingExpression(Slider.ValueProperty);
            binding?.UpdateSource();
        }
    }
}
