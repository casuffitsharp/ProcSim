using System.Globalization;
using System.Windows.Data;

namespace ProcSim.Converters;

public class EqualityToBooleanConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values?.Length == 2)
            return values[0] != null && values[0].Equals(values[1]);

        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
