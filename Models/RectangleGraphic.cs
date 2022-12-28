using System;
using System.Collections.Generic;
using Mapsui.Nts;
using NetTopologySuite.Geometries;

namespace map_app.Models
{
    public class RectangleGraphic : BaseGraphic
    {
        public RectangleGraphic(List<Coordinate> points) : base(points)
        {            
            if (points.Count != 2)
                throw new ArgumentException();
        }
        
        public RectangleGraphic(GeometryFeature geometryFeature) : base(geometryFeature) { }
        public RectangleGraphic(Geometry? geometry) : base(geometry) { }

        public override GraphicType Type => GraphicType.Rectangle;

        protected override Geometry RenderGeomerty(List<Coordinate> points)
        {
            var startPos = points[0];
            var currentPos = points[1];

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