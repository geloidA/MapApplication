using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;

namespace map_app.Models.Extensions
{
    public static class LinearPointExtensions
    {
        public static IEnumerable<Coordinate> ToCoordinates(this IEnumerable<LinearPoint> source)
            => source.Select(x => x.ToCoordinate());
    }
}