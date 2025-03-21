using MaterialDesignThemes.Wpf;
using ProcSim.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ProcSim.Views;

public partial class MainView : Window
{
    private readonly MainViewModel _mainViewModel;

    public MainView()
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

    private void Slider_DragCompleted(object sender, object e)
    {
        if (sender is Slider slider)
        {
            BindingExpression binding = slider.GetBindingExpression(Slider.ValueProperty);
            binding?.UpdateSource();
        }
    }
}
