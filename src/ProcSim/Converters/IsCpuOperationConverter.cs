using System.Globalization;
using System.Windows.Data;
using ProcSim.Core.Models.Operations;

namespace ProcSim.Converters;

public class IsCpuOperationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isCpu)
            return isCpu ? "CPU" : "IO";
        
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is string str && str == "CPU";
    }
}
