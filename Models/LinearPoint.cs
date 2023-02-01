using System;
using Mapsui.Projections;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace map_app.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class LinearPoint : IThreeDimensionalPoint
    {
        private const double Eps = 1e-5;

        [JsonProperty]
        public double X { get; set; }

        [JsonProperty]
        public double Y { get; set; }

        [JsonProperty]
        public double Z { get; set; }

        public double First { get => X; set => X = value; }
        public double Second { get => Y; set => Y = value; }
        public double Third { get => Z; set => Z = value; }

        public LinearPoint() : this(0, 0, 0) { }
        public LinearPoint(double x, double y) : this(x, y, 0) { }

        public LinearPoint(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Coordinate ToCoordinate()
        {
            return new Coordinate3D(X, Y, Z);
        }

        public GeoPoint ToGeoPoint()
        {
            var lonLat = SphericalMercator.ToLonLat(X, Y);
            return new GeoPoint(lonLat.lon, lonLat.lat, Z); 
        }

        public override string ToString()
        {
            return $"X:{X} Y:{Y} Z:{Z}";
        }

        public override bool Equals(object? obj)
        {
            if (obj is not LinearPoint other) return false;
            if (ReferenceEquals(this, other)) return true;
            
            return Math.Abs(other.X - X) < Eps
                && Math.Abs(other.Y - Y) < Eps
                && Math.Abs(other.Z - Z) < Eps;
        }

        public override int GetHashCode()
        {
            return (X.GetHashCode() * 31 + Y.GetHashCode()) * 31 + Z.GetHashCode();
        }
    }
}