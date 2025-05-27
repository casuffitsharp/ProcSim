using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace ProcSim.Converters;

public sealed class EnumDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter = null, CultureInfo culture = null)
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
        // Caso o valor seja uma única string, buscamos o enum correspondente.
        if (value is string description)
        {
            Type enumType = targetType.IsEnum ? targetType : Nullable.GetUnderlyingType(targetType);
            if (enumType?.IsEnum == true)
            {
                foreach (object enumValue in Enum.GetValues(enumType))
                {
                    FieldInfo field = enumType.GetField(enumValue.ToString());
                    DescriptionAttribute attr = field?.GetCustomAttribute<DescriptionAttribute>();
                    if (attr?.Description == description)
                        return enumValue;

                    // Se não tiver descrição definida, compara com o ToString() do enum.
                    if (string.IsNullOrEmpty(attr?.Description) && enumValue.ToString() == description)
                        return enumValue;
                }
            }
        }

        // Caso o valor seja uma coleção (IEnumerable de strings) e o targetType seja uma coleção genérica
        if (value is IEnumerable enumerable && targetType.IsGenericType)
        {
            // Obtém o tipo genérico esperado na coleção (ex.: IoDeviceType)
            Type genericType = targetType.GetGenericArguments()[0];
            if (!genericType.IsEnum)
                return Binding.DoNothing;

            Type listType = typeof(List<>).MakeGenericType(genericType);
            IList list = (IList)Activator.CreateInstance(listType);
            foreach (object item in enumerable)
            {
                if (item is not string desc)
                    continue;

                foreach (object enumValue in Enum.GetValues(genericType))
                {
                    FieldInfo field = genericType.GetField(enumValue.ToString());
                    DescriptionAttribute attr = field?.GetCustomAttribute<DescriptionAttribute>();
                    if (attr?.Description == desc)
                    {
                        list.Add(enumValue);
                        break;
                    }

                    if (string.IsNullOrEmpty(attr?.Description) && enumValue.ToString() == desc)
                    {
                        list.Add(enumValue);
                        break;
                    }
                }
            }

            return list;
        }

        return Binding.DoNothing;
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
