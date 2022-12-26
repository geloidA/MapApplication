using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Nts;
using Mapsui.Styles;
using map_app.Editing.Extensions;
using NetTopologySuite.Geometries;

namespace map_app.Models
{
    public abstract class BaseGraphic : GeometryFeature
    {
        protected List<Coordinate> _linearPoints = new();

        public BaseGraphic(List<Coordinate> points) : base() 
        { 
            _linearPoints = points;
            Geometry = RenderGeomerty(points);
        }
        public BaseGraphic(GeometryFeature geometryFeature) : base(geometryFeature) { }
        public BaseGraphic(Geometry? geometry) : base(geometry) { }


        public Dictionary<string, IUserTag>? UserTags { get; set; }
        public Color? Color { get; set; } // todo: auto generate style when change color
        public double Opacity { get; set; }
        public IReadOnlyList<GeoPoint> GeoPoints => _linearPoints.Select(x => x.ToGeoPoint()).ToList();

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
                Geometry = RenderGeomerty(_linearPoints);
            }
        }
        
        public new Geometry? Geometry
        {
            get => base.Geometry;
            protected set => base.Geometry = value;
        }

        protected abstract Geometry RenderGeomerty(List<Coordinate> points);
        
        public void RerenderGeometry()
        {
            Geometry = RenderGeomerty(_linearPoints);
        }
    }
}