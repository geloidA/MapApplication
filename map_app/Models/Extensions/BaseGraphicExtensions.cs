using map_app.Editing.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace map_app.Models.Extensions;

public static class BaseGraphicExtensions
{
    public static IEnumerable<Segment> GetSegments(this BaseGraphic graphic)
    {
        if (graphic is PointGraphic || graphic.Geometry is null)
            return Enumerable.Empty<Segment>();
        var geoPoints = GetGraphicGeometryPoints(graphic);
        return geoPoints.Zip(geoPoints.Skip(1), (a, b) => new Segment(a, b));
    }

    private static IEnumerable<GeoPoint> GetGraphicGeometryPoints(BaseGraphic graphic)
    {
        if (graphic is OrthodromeGraphic)
            return graphic.GeoPoints;
        if (graphic is PolygonGraphic && graphic.Geometry!.Coordinates.Length == 3) // When polygon have two coordinates. Polygon geometry is ring.
            return graphic.Geometry.Coordinates
                .SkipLast(1)
                .Select(x => x.ToGeoPoint());
        return graphic.Geometry!.Coordinates.Select(x => x.ToGeoPoint());
    }
}