using ProcSim.Core.Enums;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ProcSim.Converters;

public class StateHistoryToBrushConverter : IMultiValueConverter
{
    private static Color ConvertHex(string hex)
    {
        return (Color)ColorConverter.ConvertFromString(hex);
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2)
            return Brushes.Transparent;

        if (values[0] is not IList<ProcessState> stateHistory)
            return Brushes.Transparent;

        // O segundo valor é o Header da coluna (string contendo o índice)
        if (!int.TryParse(values[1]?.ToString(), out int index))
            return Brushes.Transparent;

        if (index < 0 || index >= stateHistory.Count)
            return Brushes.Transparent;

        ProcessState state = stateHistory[index];
        return state switch
        {
            ProcessState.Ready => new SolidColorBrush(ConvertHex("#C5E1A5")),     // Green 200
            ProcessState.Running => new SolidColorBrush(ConvertHex("#90CAF9")),   // Blue 200
            ProcessState.Blocked => new SolidColorBrush(ConvertHex("#EF9A9A")),   // Red 200
            ProcessState.Completed => new SolidColorBrush(ConvertHex("#E0E0E0")), // Grey 300
            _ => Brushes.Transparent,
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
