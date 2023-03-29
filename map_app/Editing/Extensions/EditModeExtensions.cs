using System;

namespace map_app.Editing.Extensions;

public static class EditModeExtensions
{
    public static EditMode GetAddMode(this EditMode drawingMode)
    {
        return drawingMode switch
        {
            EditMode.DrawingOrthodromeLine => EditMode.AddOrthodromeLine,
            EditMode.DrawingPolygon => EditMode.AddPolygon,
            EditMode.DrawingRectangle => EditMode.AddRectangle,
            _ => throw new NotImplementedException()
        };
    }
}