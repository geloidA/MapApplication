using Mapsui.Styles;

namespace map_app.Services;

public class VectorStyleHelper
{
    /// <summary>
    /// Copy feilds from source style to target and returns target
    /// </summary>
    /// <param name="target"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public static T CopyFields<T>(T target, VectorStyle source) where T : VectorStyle
    {
        target.Opacity = source.Opacity;
        target.Outline = source.Outline != null ? new Pen(source.Outline.Color, source.Outline.Width) : null;
        target.Fill = source.Fill != null ? new Brush(source.Fill) : null;
        target.MinVisible = source.MinVisible;
        target.MaxVisible = source.MaxVisible;
        target.Enabled = source.Enabled;
        target.Line = source.Line != null ? new Pen(source.Line.Color, source.Line.Width) : null;
        return target;
    }
}