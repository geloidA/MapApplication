using Mapsui.Nts.Extensions;
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

        public Coordinate ToGeoCoordinate()
        {
            return new Coordinate(Longtitude, Latitude);
        }

        public Coordinate ToWorldPosition()
        {
            return SphericalMercator.FromLonLat(Longtitude, Latitude).ToCoordinate();
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