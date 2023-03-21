using Mapsui.Projections;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace map_app.Models;

[JsonObject(MemberSerialization.OptIn)]
public class LinearPoint : IThreeDimensionalPoint
{
    [JsonProperty]
    public double X { get; set; }

    [JsonProperty]
    public double Y { get; set; }

    [JsonProperty]
    public double Z { get; set; }

    public double First { get => X; set => X = value; }
    public double Second { get => Y; set => Y = value; }
    public double Third { get => Z; set => Z = value; }

    public LinearPoint()
    {
    }
    
    public LinearPoint(double x, double y) : this(x, y, 0) { }

    public LinearPoint(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Coordinate ToCoordinate() => new Coordinate3D(X, Y, Z);

    public GeoPoint ToGeoPoint()
    {
        var lonLat = SphericalMercator.ToLonLat(X, Y);
        return new GeoPoint(lonLat.lon, lonLat.lat, Z); 
    }

    public override string ToString() => $"X:{X:0.00} Y:{Y:0.00} Z:{Z:0.00}";
}