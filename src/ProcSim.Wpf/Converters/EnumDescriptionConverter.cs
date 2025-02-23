using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace ProcSim.Wpf.Converters;

public class EnumDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Enum enumValue)
        {
            return GetEnumDescription(enumValue);
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing; // Não há necessidade de converter de volta
    }

    private static string GetEnumDescription(Enum value)
    {
        FieldInfo field = value.GetType().GetField(value.ToString());
        DescriptionAttribute attribute = field?.GetCustomAttributes<DescriptionAttribute>().FirstOrDefault();
        return attribute?.Description ?? value.ToString();
    }
}
