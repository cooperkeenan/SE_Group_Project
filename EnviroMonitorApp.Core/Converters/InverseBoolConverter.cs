namespace EnviroMonitorApp.Converters;

using System;
using System.Globalization;
using Microsoft.Maui.Controls;

public class InverseBoolConverter : IValueConverter   // ← add **public**
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => (value is bool b) ? !b : value!;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => (value is bool b) ? !b : value!;
}
