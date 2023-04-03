using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;

namespace map_app.Models.Extensions;

public static class LinearPointExtensions
{
    public static IEnumerable<Coordinate> ToCoordinates(this IEnumerable<LinearPoint> source)
        => source.Select(x => x.ToCoordinate());

    public static IEnumerable<GeoPoint> ToGeoPoints(this IEnumerable<LinearPoint> source)
        => source.Select(x => x.ToGeoPoint());
}