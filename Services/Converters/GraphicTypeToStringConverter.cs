using System;
using System.Globalization;
using Avalonia.Data.Converters;
using map_app.Models;

namespace map_app.Services.Converters
{
    public class GraphicTypeToStringConverter : IValueConverter // todo: rework with EnumDesctiptionConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not GraphicType graphicType)
                throw new ArgumentException("value is not GraphisType");
            return graphicType switch
            {
                GraphicType.Orthodrome => "Ортодромия",
                GraphicType.Point => "Точка",
                GraphicType.Polygon => "Полигон",
                GraphicType.Rectangle => "Прямоугольник",
                _ => throw new NotImplementedException()
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }
}