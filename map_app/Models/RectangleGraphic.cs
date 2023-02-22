using System;
using System.Collections.Generic;
using Mapsui.Nts;
using NetTopologySuite.Geometries;

namespace map_app.Models
{
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
            Geometry = RenderGeometry();
        }
        
        public RectangleGraphic(GeometryFeature geometryFeature) : base(geometryFeature) { }
        public RectangleGraphic(Geometry? geometry) : base(geometry) { }

        public override GraphicType Type => GraphicType.Rectangle;

        public Coordinate? HoverVertex { get; set; }

        public override RectangleGraphic Copy()
        {
            return new RectangleGraphic(this);
        }

        protected override Geometry RenderGeometry()
        {
            var startPos = _coordinates[0];
            var currentPos = HoverVertex ?? _coordinates[1];

            return new Polygon(new LinearRing(new[] 
            {
                new Coordinate { X = startPos.X, Y = startPos.Y },
                new Coordinate { X = currentPos.X, Y = startPos.Y },
                new Coordinate { X = currentPos.X, Y = currentPos.Y },
                new Coordinate { X = startPos.X, Y = currentPos.Y },
                new Coordinate { X = startPos.X, Y = startPos.Y } // need to be ring
            }));
        }

        public void AddPoint(Coordinate worldCoordinate)
        {
            _coordinates.Add(worldCoordinate);
        }
    }
}