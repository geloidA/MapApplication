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
        private Orthodrome _orthodrome = new();

        public OrthodromeGraphic() : base() { }
        public OrthodromeGraphic(GeometryFeature geometryFeature) : base(geometryFeature) { }
        public OrthodromeGraphic(Geometry? geometry) : base(geometry) { }

        public void AddLinearPoint(Coordinate worldCoordinate)
        {
            if (_orthodrome.Start is null)
            {
                _orthodrome.Start = worldCoordinate.ToGeoPoint();
            }
            else if (_orthodrome.End is null)
            {
                _orthodrome.End = worldCoordinate.ToGeoPoint();
            }
            else
            {
                var last = _orthodrome;
                while(last.Next != null)
                {
                    last = last.Next;
                }
                last.Next = new Orthodrome { Start = last.End, End = worldCoordinate.ToGeoPoint() };
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
            var result = new List<GeoPoint>();
            var next = _orthodrome;
            if (_orthodrome.End is null || _orthodrome.Start is null)
                return new LineString(new Coordinate[0]);
            while(next != null)
            {
                result.AddRange(next.Value);
                next = next.Next;
            }            
            return new LineString(result.ToWorldPositions().ToArray());
        }
    }
}