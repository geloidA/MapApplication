using System;
using System.Collections.Generic;
using map_app.Editing.Extensions;
using map_app.Models;
using Mapsui.Utilities;
using NetTopologySuite.Geometries;

namespace map_app.Services
{
    public static class MapAlgorithms // todo: bug when draw right to left
    {
        public static List<GeoPoint> GetOrthodromePath(Coordinate worldPoint1, Coordinate worldPoint2)
        {
            return GetOrthodromePath(worldPoint1.ToGeoPoint(), worldPoint2.ToGeoPoint());
        }

        public static List<GeoPoint> GetOrthodromePath(GeoPoint? degPoint1, GeoPoint? degPoint2, double lineStep = 0.5)
        {
            if (degPoint1 is null) return new List<GeoPoint>();
            if (degPoint2 is null) return new List<GeoPoint>();
            var comparer = new ThreeDimentionalPointEqualityComparer();
            if (comparer.Equals(degPoint1, degPoint2)) return new List<GeoPoint>();

            var lat1Rad = Algorithms.DegreesToRadians(degPoint1.Latitude);
            var lat2Rad = Algorithms.DegreesToRadians(degPoint2.Latitude);
            var lon1Rad = Algorithms.DegreesToRadians(degPoint1.Longtitude);
            var lon2Rad = Algorithms.DegreesToRadians(degPoint2.Longtitude);

            var points = new List<GeoPoint>();
            var left = Math.Min(degPoint1.Longtitude, degPoint2.Longtitude);
            var right = Math.Max(degPoint1.Longtitude, degPoint2.Longtitude);
            for (var lon = left; lon <= right; lon += lineStep)
            {  
                var lonRad = Algorithms.DegreesToRadians(lon);
                var lat = Math.Atan((Math.Tan(lat1Rad) * Math.Sin(lon2Rad - lonRad)) / (Math.Sin(lon2Rad - lon1Rad)) + (Math.Tan(lat2Rad) * Math.Sin(lonRad - lon1Rad)) / (Math.Sin(lon2Rad - lon1Rad)));
                points.Add(new GeoPoint(lon, lat / Math.PI * 180.0));
            }
            points.Add(right == degPoint1.Longtitude ? degPoint1 : degPoint2);
            return points;
        }
    }
}