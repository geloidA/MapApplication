using System;
using Mapsui.Projections;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace map_app.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GeoPoint : IThreeDimensionalPoint
    {
        [JsonProperty("Lon")]
        public double Longtitude { get; set; }

        [JsonProperty("Lat")]
        public double Latitude { get; set; }

        [JsonProperty("Alt")]
        public double Altitude { get; set; }
        
        public double First  { get => this.Longtitude; set => this.Longtitude = value; }
        public double Second { get => this.Latitude; set => this.Latitude = value; }
        public double Third { get => this.Altitude; set => this.Altitude = value; }

        public GeoPoint() : this(0, 0, 0) { }

        public GeoPoint(double lon, double lat) : this(lon, lat, 0) { }

        public GeoPoint(double lon, double lat, double alt)
        {
            Longtitude = lon;
            Latitude = lat;
            Altitude = alt;
        }

        public Point ToPoint()
        {
            var point = SphericalMercator.FromLonLat(Longtitude, Latitude);
            return new Point(point.x, point.y, Altitude);
        }

        public Coordinate ToWorldPosition()
        {
            var coordinate = SphericalMercator.FromLonLat(Longtitude, Latitude);
            return new Coordinate3D(coordinate.x, coordinate.y, Altitude);
        }

        public LinearPoint ToLinearPoint()
        {
            var coordinate = this.ToWorldPosition();
            return new LinearPoint(
                coordinate.X,
                coordinate.Y,
                coordinate.Z
            );
        }

        public GeoPoint Copy()
        {
            return new GeoPoint(Longtitude, Latitude, Altitude);
        }

        public override string ToString()
        {
            return string.Format("Lon:{0:0.00} ; Lat:{1:0.00} ; Alt:{2:0.00}", Longtitude, Latitude, Altitude);
        }
    }
}