using System.Collections.Generic;
using map_app.Editing.Extensions;
using map_app.Models.Extensions;
using map_app.Services;
using System.Linq;
using Mapsui.Nts;
using NetTopologySuite.Geometries;

namespace map_app.Models
{
    public class OrthodromeGraphic : BaseGraphic, IStepByStepGraphic
    {
        public OrthodromeGraphic() : base() { }
        public OrthodromeGraphic(GeometryFeature geometryFeature) : base(geometryFeature) { }
        public OrthodromeGraphic(Geometry? geometry) : base(geometry) { }

        public void AddLinearPoint(Coordinate worldCoordinate)
        {
            _linearPoints.Add(worldCoordinate);
            Geometry = RenderGeomerty(_linearPoints);
        }

        public void AddRangeLinearPoints(IEnumerable<Coordinate> worldCoordinates)
        {
            throw new System.NotImplementedException();
        }

        public Geometry RenderStepGeometry(Coordinate worldPosition)
        {
            throw new System.NotImplementedException();
        }

        protected override Geometry RenderGeomerty(List<Coordinate> points)
        {
            var result = new List<GeoPoint>();
            for (var i = 0; i < points.Count - 2; i++) // todo: 2 because we need escape last copied point
            {
                var orthodrome = MapAlgorithms.GetOrthodromePath(points[i], points[i + 1]);
                result.AddRange(orthodrome);
            }
            return new LineString(result.ToWorldPositions().ToArray());
        }
    }
}