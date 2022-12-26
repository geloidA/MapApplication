using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;

namespace map_app.Models.Extensions
{
    public static class GeoPointExtensions
    {
        public static IEnumerable<Coordinate> ToWorldPositions(this IEnumerable<GeoPoint> source)
             => source.Select(x => x.ToWorldPosition());
    }
}