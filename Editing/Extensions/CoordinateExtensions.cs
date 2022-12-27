using System.Collections.Generic;
using System.Linq;
using map_app.Models;
using Mapsui;
using Mapsui.Nts.Extensions;
using Mapsui.Projections;
using NetTopologySuite.Geometries;

namespace map_app.Editing.Extensions
{
    public static class CoordinateExtensions
    {
        public static void SetXY(this Coordinate? target, Coordinate? source)
        {
            if (target is null) return;
            if (source is null) return;

            target.X = source.X;
            target.Y = source.Y;
        }

        public static void SetXY(this Coordinate? target, MPoint? source)
        {
            if (target is null) return;
            if (source is null) return;

            target.X = source.X;
            target.Y = source.Y;
        }

        public static GeoPoint ToGeoPoint(this Coordinate target)
        {
            var lonLat = SphericalMercator.ToLonLat(target.X, target.Y);
            return new GeoPoint(lonLat.lon, lonLat.lat);
        }

        public static LinearPoint ToLinearPoint(this Coordinate target)
        {
            return new LinearPoint(target.X, target.Y, target.Z);
        }

        public static IEnumerable<Coordinate> LonLatToWorld(this IList<Coordinate> points) 
            => points.Select(p => SphericalMercator.FromLonLat(p.X, p.Y).ToCoordinate());            
    }
}