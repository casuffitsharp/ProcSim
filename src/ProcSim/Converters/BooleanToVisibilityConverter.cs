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

        if (parameter != null && bool.Parse((string)parameter))
            flag = !flag;

        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool back = value is Visibility visibility && visibility == Visibility.Visible;
        if (parameter != null && (bool)parameter)
            back = !back;

        return back;
    }
}