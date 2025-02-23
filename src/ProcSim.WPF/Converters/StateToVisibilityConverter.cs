using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ProcSim.Core.Enums;

namespace ProcSim.WPF.Converters;

public class StateToVisibilityConverter(ProcessState targetState) : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is ProcessState state && state == targetState ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
