using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace ProcSim.Core.Extensions;

public static partial class JsonExtensions
{
    public static void InheritJsonIgnore(this JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Kind is not JsonTypeInfoKind.Object)
            return;

        List<int> indicesToRemove = [];
        for (int i = 0; i < jsonTypeInfo.Properties.Count; i++)
        {
            if (jsonTypeInfo.Properties[i].AttributeProvider is not PropertyInfo propertyInfo)
                continue;

            if (propertyInfo.GetCustomAttribute<JsonIgnoreAttribute>() is null)
                continue;

            indicesToRemove.Add(i);
        }

        for (int i = indicesToRemove.Count - 1; i >= 0; i--)
            jsonTypeInfo.Properties.RemoveAt(indicesToRemove[i]);
    }
}