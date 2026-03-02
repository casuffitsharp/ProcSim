using System.Globalization;
using System.Windows.Data;

namespace ProcSim.Converters;

[ValueConversion(typeof(bool), typeof(bool))]
public class InvertBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return InvertBoolean(value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return InvertBoolean(value);
    }

    private static bool InvertBoolean(object value)
    {
        bool original = (bool)value;
        return !original;
    }
}