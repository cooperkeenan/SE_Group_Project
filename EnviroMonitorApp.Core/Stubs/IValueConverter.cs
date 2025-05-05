// This stub lets Core compile on plain .NET 8 without pulling the MAUI workload.
// It’s excluded from the real mobile build because Core isn't referenced by the head-project at runtime.
namespace Microsoft.Maui.Controls
{
    public interface IValueConverter
    {
        object Convert     (object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture);
        object ConvertBack (object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture);
    }
}
