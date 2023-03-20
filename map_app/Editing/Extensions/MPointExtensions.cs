using map_app.Models;
using Mapsui;

namespace map_app.Editing.Extensions;

public static class MPointExtensions
{
    public static Coordinate3D ToCoordinate3D(this MPoint point) => new Coordinate3D(point.X, point.Y, 0);
}