using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace ProcSim.Core.Old.Extensions;

public static partial class JsonExtensions
{
    public static void InheritJsonIgnore(this JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Kind is not JsonTypeInfoKind.Object)
            return;

        for (int i = 0; i < jsonTypeInfo.Properties.Count; i++)
        {
            if (jsonTypeInfo.Properties[i].AttributeProvider is not PropertyInfo propertyInfo)
                continue;

            if (propertyInfo.GetCustomAttribute<JsonIgnoreAttribute>() is null)
                continue;

            jsonTypeInfo.Properties.RemoveAt(i--);
        }
    }
}