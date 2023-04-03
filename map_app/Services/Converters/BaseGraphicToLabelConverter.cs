using Avalonia.Data.Converters;
using map_app.Models;
using map_app.Services.Attributes;
using System;
using System.Globalization;
using System.Linq;

namespace map_app.Services.Converters;

public class BaseGraphicToLabelConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not BaseGraphic graphic) throw new NotImplementedException("Can only convert BaseGraphic");
        return graphic
            .GetType()
            .GetCustomAttributes(typeof(LabelAttribute), false)
            .Cast<LabelAttribute>()
            .First().Text;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}