using System;
using System.Collections.Generic;
using Mapsui.Nts;
using Mapsui.Nts.Extensions;
using NetTopologySuite.Geometries;

namespace map_app.Models
{
    public class PointGraphic : BaseGraphic
    {
        public PointGraphic(List<Coordinate> points) : base(points) { }
        public PointGraphic(GeometryFeature geometryFeature) : base(geometryFeature) { }
        public PointGraphic(Geometry? geometry) : base(geometry) { }

        protected override Geometry RenderGeomerty(List<Coordinate> points)
        {
            if (points.Count != 1)
                throw new ArgumentException($"List need contain one point, but was {points.Count}");
            var coordinate = points[0] ?? throw new NullReferenceException("Point was null");
            return coordinate.ToPoint();
        }
    }
}