using System.Collections.Generic;
using map_app.Editing.Extensions;
using map_app.Models.Extensions;
using System.Linq;
using Mapsui.Nts;
using NetTopologySuite.Geometries;

namespace map_app.Models
{
    public class OrthodromeGraphic : BaseGraphic, IStepByStepGraphic
    {
        private Orthodrome? _orthodrome;

        public OrthodromeGraphic(List<Coordinate> points) : base(points) { }
        public OrthodromeGraphic(GeometryFeature geometryFeature) : base(geometryFeature) { }
        public OrthodromeGraphic(Geometry? geometry) : base(geometry) { }

        public void AddLinearPoint(Coordinate worldCoordinate)
        {
            var newPoint = worldCoordinate.ToGeoPoint();
            if (_orthodrome is null)
            {
                _orthodrome = new Orthodrome(newPoint, null);
            }
            else
            {
                var last = _orthodrome;
                while(last.Next != null)
                {
                    last = last.Next;
                }
                last.Next = new Orthodrome(last.End, newPoint);
            }
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
            var last = points.Last();
            var result = new List<GeoPoint>();
            var next = _orthodrome;
            if (_orthodrome?.Start is null)
                return new LineString(new Coordinate[0]);
            while(next != null)
            {
                if (next.Next is null) // change last point while mouse moving
                {
                    next.End = last.ToGeoPoint();
                }
                result.AddRange(next.Path);
                next = next.Next;
            }            
            return new LineString(result.ToWorldPositions().ToArray());
        }
    }
}