using Mapsui.Styles;

namespace map_app.Services.Renders;

public class LabelDistanceStyle : IStyle
{
    public double MinVisible { get; set; } = 0;
    public double MaxVisible { get; set; } = double.MaxValue;
    public bool Enabled { get; set; }
    public float Opacity { get; set; }
}