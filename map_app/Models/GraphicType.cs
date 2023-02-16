using System.ComponentModel;

namespace map_app.Models
{
    public enum GraphicType
    {
        [Description("Точка")]
        Point,
        [Description("Прямоугольник")]
        Rectangle,
        [Description("Ортодромия")]
        Orthodrome,
        [Description("Полигон")]
        Polygon
    }
}