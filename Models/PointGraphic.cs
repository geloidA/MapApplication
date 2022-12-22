using System;
using System.Collections.Generic;
using Mapsui.Nts.Extensions;
using NetTopologySuite.Geometries;

namespace map_app.Models
{
    public class PointGraphic : BaseGraphic
    {
        protected override Geometry ConstructGeomerty(List<Coordinate> points)
        {
            if (points.Count != 1)
                throw new ArgumentException($"List need contain one point, but was {points.Count}");
            var coordinate = points[0] ?? throw new NullReferenceException("Point was null");
            return coordinate.ToPoint();
        }
    }
}