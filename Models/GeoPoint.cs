using Mapsui.Projections;
using NetTopologySuite.Geometries;

namespace map_app.Models
{
    public class GeoPoint
    {
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
            return new Point(point.x, point.y);
        }
    }
}