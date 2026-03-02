using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ProcSim.Converters;

public sealed class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool flag = false;
        if (value is bool v)
        {
            flag = v;
        }

        if (GetBoolParameter(parameter))
            flag = !flag;

        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool back = value is Visibility visibility && visibility == Visibility.Visible;
        if (GetBoolParameter(parameter))
            back = !back;

        return back;
    }

    private static bool GetBoolParameter(object parameter)
    {
        if (parameter is bool b)
            return b;

        return parameter is string s && bool.TryParse(s, out bool result) && result;
    }
}