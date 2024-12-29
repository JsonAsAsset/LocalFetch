using Avalonia.Data.Converters;
using System;
using System.Globalization;
using Avalonia.Media;

namespace LocalFetch.Converters;

public class StatusColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Convert the Status enum to a color brush
        return value switch
        {
            EApplicationStatus.Loading => new SolidColorBrush(Color.Parse("#ffb300")),
            EApplicationStatus.Completed => new SolidColorBrush(Color.Parse("#008cff")),
            _ => Brushes.Transparent
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}