using MaterialDesignThemes.Wpf;
using ProcSim.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ProcSim.Views;

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

    private void Slider_DragCompleted(object sender, object e)
    {
        if (sender is Slider slider)
        {
            System.Windows.Data.BindingExpression binding = slider.GetBindingExpression(Slider.ValueProperty);
            binding?.UpdateSource();
        }
    }
}
