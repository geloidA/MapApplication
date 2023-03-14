using System;
using System.Globalization;
using Avalonia.Data.Converters;
using map_app.Models;

namespace map_app.Services.Converters
{
    public class GraphicTypeToBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not GraphicType graphicType)
                throw new ArgumentException("value is not GraphisType");
            return graphicType == GraphicType.Point;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}