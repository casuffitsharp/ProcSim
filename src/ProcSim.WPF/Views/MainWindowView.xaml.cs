using MaterialDesignThemes.Wpf;
using ProcSim.Wpf.ViewModels;
using System.Windows;

namespace ProcSim.Wpf.Views;

public partial class MainWindowView : Window
{
    private readonly MainViewModel _schedulerViewModel;

    public MainWindowView()
    {
        InitializeComponent();

        _schedulerViewModel = new MainViewModel();
        DataContext = _schedulerViewModel;
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
