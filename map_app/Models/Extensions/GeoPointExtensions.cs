using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;

namespace map_app.Models.Extensions;

public static class GeoPointExtensions
{
    public static IEnumerable<Coordinate> ToWorldPositions(this IEnumerable<GeoPoint> source)
        => source.Select(x => x.ToWorldPosition());

    public static IEnumerable<LinearPoint> ToLinearPoints(this IEnumerable<GeoPoint> source)
        => source.Select(x => x.ToLinearPoint());
}