using MaterialDesignThemes.Wpf;
using ProcSim.Wpf.Helpers;
using ProcSim.Wpf.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ProcSim.Wpf.Views;

public partial class MainWindowView : Window
{
    private readonly MainViewModel _mainViewModel;
    private int _currentDynamicColumns = 0;

    public MainWindowView()
    {
        InitializeComponent();

        _mainViewModel = new MainViewModel();
        DataContext = _mainViewModel;
        _mainViewModel.GanttUpdated += OnGanttUpdated;
    }

    private void OnGanttUpdated(int totalTimeUnits)
    {
        UpdateCurrentGanttColumn(totalTimeUnits);
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

    public void UpdateCurrentGanttColumn(int totalTimeUnits)
    {
        // A coluna 0 é fixa. As demais são dinâmicas.
        if (totalTimeUnits > _currentDynamicColumns)
        {
            for (int t = _currentDynamicColumns; t < totalTimeUnits; t++)
            {
                var col = new DataGridTemplateColumn
                {
                    Header = t.ToString(), // Você ainda pode exibir o índice se quiser
                    CellTemplate = (DataTemplate)FindResource("GanttCellTemplate")
                };

                // Define a attached property para armazenar o índice da coluna
                DataGridColumnExtensions.SetColumnIndex(col, t);

                dataGridGantt.Columns.Add(col);
            }
            _currentDynamicColumns = totalTimeUnits;
        }
    }
}
