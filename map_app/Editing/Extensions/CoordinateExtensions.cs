using map_app.Models;
using Mapsui;
using Mapsui.Nts.Extensions;
using Mapsui.Projections;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;

namespace map_app.Editing.Extensions;

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

    public static Coordinate3D ToCoordinate3D(this Coordinate source)
        => new(source.X, source.Y, 0);

    public static GeoPoint ToGeoPoint(this Coordinate target)
    {
        var (lon, lat) = SphericalMercator.ToLonLat(target.X, target.Y);
        return target is Coordinate3D c
            ? new GeoPoint(lon, lat, c.Z)
            : new GeoPoint(lon, lat);
    }

    public static IEnumerable<GeoPoint> ToGeoPoints(this IEnumerable<Coordinate> target)
        => target.Select(c => c.ToGeoPoint());

    public static LinearPoint ToLinearPoint(this Coordinate target)
        => new(target.X, target.Y, target.Z);

    public static IEnumerable<Coordinate> LonLatToWorld(this IList<Coordinate> points)
        => points.Select(p => SphericalMercator.FromLonLat(p.X, p.Y).ToCoordinate());
}