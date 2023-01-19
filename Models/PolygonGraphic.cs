using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Nts;
using NetTopologySuite.Geometries;

namespace map_app.Models
{
    public class PolygonGraphic : BaseGraphic, IStepByStepGraphic
    {
        public PolygonGraphic(List<Coordinate> points) : base(points) 
        {
            if (points.Count < 2)
                throw new ArgumentException("Points length can't be less then 2");
        }

        public PolygonGraphic(GeometryFeature geometryFeature) : base(geometryFeature) { }
        public PolygonGraphic(Geometry? geometry) : base(geometry) { }

        public override GraphicType Type => GraphicType.Polygon;

        public void AddLinearPoint(Coordinate worldPoint)
        {
            _coordinates.Add(worldPoint);
            Geometry = RenderGeometry(_coordinates);
        }

        public void AddRangeLinearPoints(IEnumerable<Coordinate> worldPoints)
        {
            foreach(var point in worldPoints)
            {
                _coordinates.Add(point);
            }
            Geometry = RenderGeometry(_coordinates);
        }

        public Geometry RenderStepGeometry(Coordinate worldPosition)
        {
            throw new NotImplementedException();
        }

        protected override Geometry RenderGeometry(List<Coordinate> points)
        {
            var linearRing = points.ToList();
            linearRing.Add(points[0].Copy()); // Add first coordinate at end to close the ring.
            return new Polygon(new LinearRing(linearRing.ToArray()));
        }
    }
}