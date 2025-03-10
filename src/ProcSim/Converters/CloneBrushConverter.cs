using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ProcSim.Converters;

public class CloneBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SolidColorBrush scb)
            // Retorna uma nova instância do brush com a mesma cor
            return new SolidColorBrush(scb.Color);
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
