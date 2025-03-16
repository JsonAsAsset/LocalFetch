using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LocalFetch.Views.Converters;

public class TruncateConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string text) return value;
        
        var maxLength = 55;
        return text.Length > maxLength ? string.Concat(text.AsSpan(0, maxLength), "..") : value;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}