using MaterialDesignThemes.Wpf;
using ProcSim.Wpf.Helpers;
using ProcSim.Wpf.ViewModels;
using System.Windows;
using System.Windows.Controls;

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
        var paletteHelper = new PaletteHelper();
        var theme = paletteHelper.GetTheme();

        theme.SetBaseTheme(isDarkTheme ? BaseTheme.Dark : BaseTheme.Light);
        paletteHelper.SetTheme(theme);
    }

    private void MenuDarkModeButton_Click(object sender, RoutedEventArgs e)
    {
        ModifyTheme(DarkModeToggleButton.IsChecked == true);
    }
}
