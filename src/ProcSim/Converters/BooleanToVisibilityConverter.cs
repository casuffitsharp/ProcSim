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
        else if (value is bool?)
        {
            bool? nullable = (bool?)value;
            flag = nullable.GetValueOrDefault();
        }
        if (parameter != null)
        {
            if (bool.Parse((string)parameter))
                flag = !flag;
        }

        if (flag)
            return Visibility.Visible;

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool back = value is Visibility visibility && visibility == Visibility.Visible;
        if (parameter != null)
        {
            if ((bool)parameter)
                back = !back;
        }
        return back;
    }
}