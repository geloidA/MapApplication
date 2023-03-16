using System;

namespace map_app.Editing
{
    [Flags]
    public enum EditMode
    {
        None = 0,
        AddPoint = 1,
        AddPolygon = 2,
        DrawingPolygon = 4,
        Modify = 8,
        Rotate = 16,
        Scale = 32,
        AddOrthodromeLine = 64,
        DrawingOrthodromeLine = 128,
        AddRectangle = 256,
        DrawingRectangle = 512,
        DrawingMode = DrawingOrthodromeLine | DrawingPolygon | DrawingRectangle
    }
}