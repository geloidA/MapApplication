using System;
using System.Linq;
using map_app.Models.Extensions;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts.Extensions;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Styles;
using SkiaSharp;

namespace map_app.Services.Renders;

public class SkiaLabelDistanceStyleRenderer : ISkiaStyleRenderer
{
    public bool Draw(SKCanvas canvas, IReadOnlyViewport viewport, 
        ILayer layer, IFeature feature, IStyle style, ISymbolCache symbolCache, long iteration)
    {
        if (layer is not GraphicsLayer graphicLayer)
            return false;
        if (style is not LabelDistanceStyle labelStyle)
            return false;
        if (labelStyle.Enabled == false)
            return false;

        foreach (var segment in graphicLayer.Features.SelectMany(x => x.GetSegments()))
        {
            var screenStart = viewport.WorldToScreen(segment.Start.ToWorldPosition().ToMPoint());
            var screenEnd = viewport.WorldToScreen(segment.End.ToWorldPosition().ToMPoint());
            var angle = Math.Atan2(screenEnd.Y - screenStart.Y, screenEnd.X - screenStart.X);
            var x = (float)(screenStart.X + screenEnd.X) / 2;
            var y = (float)(screenStart.Y + screenEnd.Y) / 2;
            canvas.RotateRadians((float)angle, x, y);
            using (var textPaint = new SKPaint())
            {
                textPaint.Color = SKColors.Black;
                textPaint.IsAntialias = true;
                textPaint.TextSize = 12;
                textPaint.TextAlign = SKTextAlign.Center;
                canvas.DrawText($"{segment.Distance:f2} km", new SKPoint(x, y), textPaint);
            }
            canvas.Restore();
            canvas.Save();
        }

        return true;
    }
}