using System;
using Mapsui.Projections;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace map_app.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GeoPoint : IThreeDimensionalPoint
    {
        private const double Eps = 1e-5;

        [JsonProperty]
        public double Longtitude { get; set; }

        [JsonProperty]
        public double Latitude { get; set; }

        [JsonProperty]
        public double Altitude { get; set; }
        
        public double First  { get => Longtitude; set => Longtitude = value; }
        public double Second { get => Latitude; set => Latitude = value; }
        public double Third { get => Altitude; set => Altitude = value; }

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

        public override bool Equals(object? obj)
        {
            if (obj is not GeoPoint other) return false;
            if (ReferenceEquals(this, other)) return true;

            return Math.Abs(other.Longtitude - Longtitude) < Eps
                && Math.Abs(other.Latitude - Latitude) < Eps
                && Math.Abs(other.Altitude - Altitude) < Eps;
        }

        public override int GetHashCode()
        {
            return (Longtitude.GetHashCode() * 31 + Latitude.GetHashCode()) 
                * 31 + Altitude.GetHashCode();
        }
    }
}