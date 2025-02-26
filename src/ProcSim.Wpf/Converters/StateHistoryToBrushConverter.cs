using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ProcSim.Core.Enums;

namespace ProcSim.Wpf.Converters;

public class StateHistoryToBrushConverter : IMultiValueConverter
{
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
            ProcessState.Ready => new SolidColorBrush(Colors.LightGreen),
            ProcessState.Running => new SolidColorBrush(Colors.LightBlue),
            ProcessState.Blocked => new SolidColorBrush(Colors.LightCoral),
            ProcessState.Completed => new SolidColorBrush(Colors.LightGray),
            _ => Brushes.Transparent,
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
