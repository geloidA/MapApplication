using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Nts;
using NetTopologySuite.Geometries;

namespace map_app.Models
{
    public class PolygonGraphic : BaseGraphic, IStepByStepGraphic
    {
        public PolygonGraphic() : base() { }
        public PolygonGraphic(GeometryFeature geometryFeature) : base(geometryFeature) { }
        public PolygonGraphic(Geometry? geometry) : base(geometry) { }

        public void AddLinearPoint(Coordinate worldPoint)
        {
            _linearPoints.Add(worldPoint);
            Geometry = RenderGeomerty(_linearPoints);
        }

        public void AddRangeLinearPoints(IEnumerable<Coordinate> worldPoints)
        {
            foreach(var point in worldPoints)
            {
                _linearPoints.Add(point);
            }
            Geometry = RenderGeomerty(_linearPoints);
        }

        public Geometry RenderStepGeometry(Coordinate worldPosition)
        {
            throw new NotImplementedException();
        }

        protected override Geometry RenderGeomerty(List<Coordinate> points)
        {
            var linearRing = points.ToList();
            linearRing.Add(points[0].Copy()); // Add first coordinate at end to close the ring.
            return new Polygon(new LinearRing(linearRing.ToArray()));
        }
    }
}