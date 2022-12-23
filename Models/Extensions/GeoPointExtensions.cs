using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using System.Threading.Tasks;

namespace map_app.Models.Extensions
{
    public static class GeoPointExtensions
    {
        public static IEnumerable<Coordinate> ToWorldPositions(this IEnumerable<GeoPoint> source)
             => source.Select(x => x.ToWorldPosition());
    }
}