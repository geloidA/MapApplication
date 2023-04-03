using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using System;
using System.Globalization;

namespace map_app.Services.Converters;

public class ColorToImmutableSolidColorBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Color color)
            return new ImmutableSolidColorBrush(color);
        throw new ArgumentException("value is not Avalonia.Media.Color");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}