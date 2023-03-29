using System;
using System.Collections.Generic;
using map_app.Services.Attributes;
using Mapsui.Nts;
using NetTopologySuite.Geometries;

namespace map_app.Models
{
    [Label("Прямоугольник")]
    public class RectangleGraphic : BaseGraphic, IHoveringGraphic
    {
        private RectangleGraphic(RectangleGraphic source) : base(source) { }

        public RectangleGraphic() : base() { }

        public RectangleGraphic(Coordinate startPoint) : base()
        {
            _coordinates = new List<Coordinate> { startPoint };
        }
        
        public RectangleGraphic(List<Coordinate> points) : base()
        {            
            if (points.Count != 2)
                throw new ArgumentException();
            _coordinates = points;
            RerenderGeometry();
        }
        
        public RectangleGraphic(GeometryFeature geometryFeature) : base(geometryFeature) { }
        public RectangleGraphic(Geometry? geometry) : base(geometry) { }

        public override GraphicType Type => GraphicType.Rectangle;

        public Coordinate? HoverVertex { get; set; }

        public override RectangleGraphic Copy() => new RectangleGraphic(this);

        protected override Geometry RenderGeometry()
        {
            var first = _coordinates[0];
            var second = HoverVertex ?? _coordinates[1];

            return new Polygon(new LinearRing(new[] 
            {
                new Coordinate { X = first.X, Y = first.Y },
                new Coordinate { X = second.X, Y = first.Y },
                new Coordinate { X = second.X, Y = second.Y },
                new Coordinate { X = first.X, Y = second.Y },
                new Coordinate { X = first.X, Y = first.Y } // need to be ring
            }));
        }

        public void AddPoint(Coordinate worldCoordinate) => _coordinates.Add(worldCoordinate);
    }
}