using map_app.Editing.Extensions;
using map_app.Models;
using Mapsui.Utilities;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;

namespace map_app.Services;

public static class MapAlgorithms
{
    private const double EarthKmRadius = 6371;

    public static IEnumerable<GeoPoint> GetOrthodromePath(Coordinate worldPoint1, Coordinate worldPoint2)
        => GetOrthodromePath(worldPoint1.ToGeoPoint(), worldPoint2.ToGeoPoint());

    public static IEnumerable<GeoPoint> GetOrthodromePath(GeoPoint degPoint1, GeoPoint degPoint2, int intervalCount = 100)
    {
        var comparer = new ThreeDimentionalPointEqualityComparer();
        if (comparer.Equals(degPoint1, degPoint2))
            yield break;
        var lat1 = Algorithms.DegreesToRadians(degPoint1.Latitude);
        var lat2 = Algorithms.DegreesToRadians(degPoint2.Latitude);
        var lon1 = Algorithms.DegreesToRadians(degPoint1.Longitude);
        var lon2 = Algorithms.DegreesToRadians(degPoint2.Longitude);
        var d = Haversine(degPoint1, degPoint2) / 6371;
        var oneInterval = 1.0f / intervalCount;
        for (var i = 0; i <= intervalCount; i++)
        {
            var fraction = oneInterval * i;
            var A = Math.Sin((1 - fraction) * d) / Math.Sin(d);
            var B = Math.Sin(fraction * d) / Math.Sin(d);
            var x = A * Math.Cos(lat1) * Math.Cos(lon1) +
                    B * Math.Cos(lat2) * Math.Cos(lon2);
            var y = A * Math.Cos(lat1) * Math.Sin(lon1) +
                    B * Math.Cos(lat2) * Math.Sin(lon2);
            var z = A * Math.Sin(lat1) + B * Math.Sin(lat2);
            var lat = Math.Atan2(z, Math.Sqrt(x * x + y * y));
            var lon = Math.Atan2(y, x);
            yield return new GeoPoint(
                    Algorithms.RadiansToDegrees(lon),
                    Algorithms.RadiansToDegrees(lat));
        }
    }

    public static double Haversine(GeoPoint p1, GeoPoint p2)
    {
        var lat1 = Algorithms.DegreesToRadians(p1.Latitude);
        var lat2 = Algorithms.DegreesToRadians(p2.Latitude);
        var lon1 = Algorithms.DegreesToRadians(p1.Longitude);
        var lon2 = Algorithms.DegreesToRadians(p2.Longitude);

        return 2 * EarthKmRadius * Math.Asin(
            Math.Sqrt(
                Math.Sin((lat1 - lat2) / 2) * Math.Sin((lat1 - lat2) / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin((lon1 - lon2) / 2) * Math.Sin((lon1 - lon2) / 2)
                ));
    }
}