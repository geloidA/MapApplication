using Mapsui.Projections;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace map_app.Models;

[JsonObject(MemberSerialization.OptIn)]
public class GeoPoint : IThreeDimensionalPoint
{
    [JsonProperty("Lon")]
    public double Longitude { get; set; }

    [JsonProperty("Lat")]
    public double Latitude { get; set; }

    [JsonProperty("Alt")]
    public double Altitude { get; set; }

    public double First { get => Longitude; set => Longitude = value; }
    public double Second { get => Latitude; set => Latitude = value; }
    public double Third { get => Altitude; set => Altitude = value; }

    public GeoPoint()
    {
    }

    public GeoPoint(double lon, double lat) : this(lon, lat, 0) { }

    public GeoPoint(double lon, double lat, double alt)
    {
        Longitude = lon;
        Latitude = lat;
        Altitude = alt;
    }

    public Coordinate ToWorldPosition()
    {
        var (x, y) = SphericalMercator.FromLonLat(Longitude, Latitude);
        return new Coordinate3D(x, y, Altitude);
    }

    public LinearPoint ToLinearPoint()
    {
        var coordinate = ToWorldPosition();
        return new LinearPoint(
            coordinate.X,
            coordinate.Y,
            coordinate.Z
        );
    }

    public GeoPoint Copy() => new(Longitude, Latitude, Altitude);

    public override string ToString() => $"Lon:{Longitude:0.00} ; Lat:{Latitude:0.00} ; Alt:{Altitude:0.00}";
}