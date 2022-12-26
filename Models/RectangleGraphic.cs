using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Nts;
using NetTopologySuite.Geometries;

namespace map_app.Models
{
    public class RectangleGraphic : BaseGraphic
    {
        public RectangleGraphic() : base() { }
        public RectangleGraphic(GeometryFeature geometryFeature) : base(geometryFeature) { }
        public RectangleGraphic(Geometry? geometry) : base(geometry) { }

        protected override Geometry RenderGeomerty(List<Coordinate> points)
        {
            if (points.Count != 2)
                throw new ArgumentException();

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