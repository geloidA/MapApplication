using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace map_app.Models
{
    public class PolygonGraphic : BaseGraphic
    {
        protected override Geometry ConstructGeomerty(List<Coordinate> points)
        {
            var linearRing = points.ToList();
            linearRing.Add(points[0]); // Add first coordinate at end to close the ring.
            return new Polygon(new LinearRing(linearRing.ToArray()));
        }
    }
}