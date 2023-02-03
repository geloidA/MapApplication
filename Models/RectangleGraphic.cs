using System;
using System.Collections.Generic;
using Mapsui.Nts;
using NetTopologySuite.Geometries;

namespace map_app.Models
{
    public class RectangleGraphic : BaseGraphic
    {
        private RectangleGraphic(RectangleGraphic source) : base(source) { }

        public RectangleGraphic() : base() { }
        
        public RectangleGraphic(List<Coordinate> points) : base(points)
        {            
            if (points.Count != 2)
                throw new ArgumentException();
        }
        
        public RectangleGraphic(GeometryFeature geometryFeature) : base(geometryFeature) { }
        public RectangleGraphic(Geometry? geometry) : base(geometry) { }

        public override GraphicType Type => GraphicType.Rectangle;

        public override RectangleGraphic LightCopy()
        {
            return new RectangleGraphic(this);
        }

        protected override Geometry RenderGeometry()
        {
            var startPos = _coordinates[0];
            var currentPos = _coordinates[1];

            return new Polygon(new LinearRing(new[] 
            {
                new Coordinate { X = startPos.X, Y = startPos.Y },
                new Coordinate { X = currentPos.X, Y = startPos.Y },
                new Coordinate { X = currentPos.X, Y = currentPos.Y },
                new Coordinate { X = startPos.X, Y = currentPos.Y },
                new Coordinate { X = startPos.X, Y = startPos.Y } // need be ring
            }));
        }
    }
}