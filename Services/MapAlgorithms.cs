using System;
using System.Collections.Generic;
using map_app.Editing.Extensions;
using map_app.Models;
using Mapsui.Utilities;
using NetTopologySuite.Geometries;

namespace map_app.Services
{
    public static class MapAlgorithms
    {
        public static List<GeoPoint> GetOrthodromePath(Coordinate worldPoint1, Coordinate worldPoint2)
        {
            return GetOrthodromePath(worldPoint1.ToGeoPoint(), worldPoint2.ToGeoPoint());
        }

        public static List<GeoPoint> GetOrthodromePath(GeoPoint degPoint1, GeoPoint degPoint2, double lineStep = 0.5)
        {
            var lat1Rad = Algorithms.DegreesToRadians(degPoint1.Latitude);
            var lat2Rad = Algorithms.DegreesToRadians(degPoint2.Latitude);
            var lon1Rad = Algorithms.DegreesToRadians(degPoint1.Longtitude);
            var lon2Rad = Algorithms.DegreesToRadians(degPoint2.Longtitude);

            var points = new List<GeoPoint>();
            var left = Math.Min(degPoint1.Longtitude, degPoint2.Longtitude) == degPoint1.Longtitude ? degPoint1 : degPoint2;
            var right = left.Longtitude == degPoint1.Longtitude ? degPoint2 : degPoint1;
            for (var lon = left.Longtitude; lon <= right.Longtitude; lon += lineStep)
            {  
                var lonRad = Algorithms.DegreesToRadians(lon);
                var lat = Math.Atan((Math.Tan(lat1Rad) * Math.Sin(lon2Rad - lonRad)) / (Math.Sin(lon2Rad - lon1Rad)) + (Math.Tan(lat2Rad) * Math.Sin(lonRad - lon1Rad)) / (Math.Sin(lon2Rad - lon1Rad)));  
                points.Add(new GeoPoint(lon, lat / Math.PI * 180.0));
            }
            points.Add(right);
            return points;
        }
    }
}