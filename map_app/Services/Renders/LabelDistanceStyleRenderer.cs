using map_app.Models;
using map_app.Models.Extensions;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts.Extensions;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Styles;
using SkiaSharp;

namespace map_app.Services.Renders;

public class LabelDistanceStyleRenderer : ISkiaStyleRenderer
{
    public bool Draw(SKCanvas canvas, IReadOnlyViewport viewport, 
        ILayer layer, IFeature feature, IStyle style, ISymbolCache symbolCache, long iteration)
    {
        if (feature.Extent is null) return false;
        if (!layer.Enabled) return false;

        var graphic = (BaseGraphic)feature;
        foreach (var segment in graphic.GetSegments())
        {
            var screenStart = viewport.WorldToScreen(segment.Start.ToWorldPosition().ToMPoint());
            var screenEnd = viewport.WorldToScreen(segment.End.ToWorldPosition().ToMPoint());
            var x = (float)(screenStart.X + screenEnd.X) / 2;
            var y = (float)(screenStart.Y + screenEnd.Y) / 2;
            DrawDistanceLabel($"{segment.Distance:f2} km", new SKPoint(x, y), canvas);
        }

        return true;
    }

    private void DrawDistanceLabel(string text, SKPoint point, SKCanvas canvas)
    {
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.TextAlign = SKTextAlign.Center;
            paint.TextSize = 10;
            paint.Style = SKPaintStyle.StrokeAndFill;
            paint.Color = SKColors.White;
            paint.StrokeWidth = 2.3f;
            canvas.DrawText(text, point, paint);
            paint.Color = SKColors.Black;
            paint.StrokeWidth = 0.5f;
            canvas.DrawText(text, point, paint);
        }
    }
}