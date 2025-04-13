using MaterialDesignThemes.Wpf;
using ProcSim.Assets;
using ProcSim.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ProcSim.Views;

public partial class MainView : Window
{
    public MainView()
    {
        InitializeComponent();

        if (Settings.Default.DarkMode)
        {
            ModifyTheme(true);
            DarkModeToggleButton.IsChecked = true;
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        MainViewModel viewModel = (MainViewModel)DataContext;
        viewModel.VmSettingsVm.SaveConfig();
        viewModel.ProcessesSettingsVm.SaveConfig();
        Settings.Default.CpuTime = viewModel.CpuTime;

        Settings.Default.Save();

        viewModel.Dispose();
        base.OnClosing(e);
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
        Settings.Default.DarkMode = DarkModeToggleButton.IsChecked == true;
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
