using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace ProcSim.Converters;

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
        if (value is string description)
        {
            return ConvertSingleDescription(description, targetType);
        }

        if (value is IEnumerable enumerable && targetType.IsGenericType)
        {
            return ConvertDescriptionCollection(enumerable, targetType);
        }

        return Binding.DoNothing;
    }

    private static object ConvertSingleDescription(string description, Type targetType)
    {
        Type enumType = targetType.IsEnum ? targetType : Nullable.GetUnderlyingType(targetType);

        if (enumType?.IsEnum != true)
            return Binding.DoNothing;

        object result = FindEnumValueByDescription(enumType, description);
        return result ?? Binding.DoNothing;
    }

    private static object ConvertDescriptionCollection(IEnumerable enumerable, Type targetType)
    {
        Type genericType = targetType.GetGenericArguments()[0];

        if (!genericType.IsEnum)
            return Binding.DoNothing;

        Type listType = typeof(List<>).MakeGenericType(genericType);
        IList list = (IList)Activator.CreateInstance(listType);

        foreach (object item in enumerable)
        {
            if (item is string desc)
            {
                object enumValue = FindEnumValueByDescription(genericType, desc);
                if (enumValue != null)
                {
                    list.Add(enumValue);
                }
            }
        }

        return list;
    }

    private static object FindEnumValueByDescription(Type enumType, string description)
    {
        foreach (object enumValue in Enum.GetValues(enumType))
        {
            if (MatchesDescription(enumType, enumValue, description))
            {
                return enumValue;
            }
        }

        return null;
    }

    private static bool MatchesDescription(Type enumType, object enumValue, string description)
    {
        FieldInfo field = enumType.GetField(enumValue.ToString());
        DescriptionAttribute attr = field?.GetCustomAttribute<DescriptionAttribute>();

        if (attr?.Description == description)
            return true;

        if (string.IsNullOrEmpty(attr?.Description) && enumValue.ToString() == description)
            return true;

        return false;
    }

    private static IEnumerable<string> ConvertCollection(IEnumerable enumerable)
    {
        return [.. enumerable.Cast<object>().OfType<Enum>().Select(GetEnumDescription)];
    }

    public static string GetEnumDescription(Enum value)
    {
        FieldInfo field = value.GetType().GetField(value.ToString());
        return field?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? value.ToString();
    }
}
