using ProcSim.Core.Enums;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace ProcSim.Wpf.Converters;

public sealed class EnumDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            IEnumerable enumerable => ConvertCollection(enumerable),
            Enum enumValue => GetEnumDescription(enumValue),
            _ => Binding.DoNothing
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }

    private static IEnumerable<string> ConvertCollection(IEnumerable enumerable)
    {
        return [.. enumerable.Cast<object>().OfType<Enum>().Select(GetEnumDescription)];
    }

    private static string GetEnumDescription(Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        return field?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? value.ToString();
    }
}
