namespace EnviroMonitorApp.Converters;

using System;
using System.Globalization;
using Microsoft.Maui.Controls;

/// <summary>
/// Converter that inverts boolean values.
/// Useful for scenarios like binding the IsEnabled property of controls to a "busy" flag,
/// where the control should be disabled when the busy flag is true.
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean value to its inverse.
    /// </summary>
    /// <param name="value">The value to convert, expected to be a boolean.</param>
    /// <param name="targetType">The type to which the value is being converted (ignored).</param>
    /// <param name="parameter">Additional parameter (ignored).</param>
    /// <param name="culture">The culture to use for conversion (ignored).</param>
    /// <returns>If the value is a boolean, returns its logical inverse; otherwise, returns the original value.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => (value is bool b) ? !b : value!;

    /// <summary>
    /// Converts a boolean value back to its inverse.
    /// </summary>
    /// <param name="value">The value to convert, expected to be a boolean.</param>
    /// <param name="targetType">The type to which the value is being converted (ignored).</param>
    /// <param name="parameter">Additional parameter (ignored).</param>
    /// <param name="culture">The culture to use for conversion (ignored).</param>
    /// <returns>If the value is a boolean, returns its logical inverse; otherwise, returns the original value.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => (value is bool b) ? !b : value!;
}