using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using NetTopologySuite.Geometries;

namespace map_app.Models
{
    public abstract class BaseGraphic : GeometryFeature
    {
        private List<Coordinate> _linearPoints = new();

        public BaseGraphic() : base() { }
        public BaseGraphic(GeometryFeature geometryFeature) : base(geometryFeature) { }
        public BaseGraphic(Geometry? geometry) : base(geometry) { }


        public Dictionary<string, IUserTag>? UserTags { get; set; }
        public Color? Color { get; set; } // todo: auto generate style when change color
        public double Opacity { get; set; }
        public IReadOnlyList<GeoPoint> GeoPoints => _linearPoints.Select(x => 
        {
            var point = SphericalMercator.ToLonLat(x.X, x.Y);
            return new GeoPoint(point.lon, point.lat);
        })
        .ToList();

        /// <summary>
        /// Recalculation Geometry property when set method is called
        /// </summary>
        public IReadOnlyList<Coordinate> LinearPoints
        {
            get => _linearPoints;
            set
            {
                if (value is null)
                    throw new ArgumentNullException();
                
                _linearPoints = value.ToList();
                Geometry = ConstructGeomerty(_linearPoints);
            }
        }
        
        public new Geometry? Geometry
        {
            get => base.Geometry;
            private set => base.Geometry = value;
        }

        protected abstract Geometry ConstructGeomerty(List<Coordinate> points);
    }
}