using System;
using Mapsui.Nts.Extensions;
using Mapsui.Projections;
using NetTopologySuite.Geometries;

namespace map_app.Models
{
    public class GeoPoint
    {
        private const double Eps = 1e-5;

        public double Longtitude { get; set; }
        public double Latitude { get; set; }
        public double Altitude { get; set; }

        public GeoPoint() : this(0, 0, 0)
        {             
        }

        public GeoPoint(double lon, double lat) : this(lon, lat, 0)
        {             
        }

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
            if (obj is not GeoPoint other)
                return false;

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