using System.Collections.Generic;
using map_app.Editing.Extensions;
using map_app.Models.Extensions;
using System.Linq;
using Mapsui.Nts;
using NetTopologySuite.Geometries;
using System;

namespace map_app.Models
{
    public class OrthodromeGraphic : BaseGraphic, IStepByStepGraphic
    {
        private Orthodrome? _orthodrome;

        public OrthodromeGraphic(List<Coordinate> points) : base(points)
        {
            var firstPoints = points.Take(2).ToArray();
            if (firstPoints.Length < 2)
                throw new ArgumentException("length can't be less 2");

            _orthodrome = new Orthodrome(firstPoints[0].ToGeoPoint(), firstPoints[1].ToGeoPoint());
            AddRangeLinearPoints(points.Skip(2));
        }

        public OrthodromeGraphic(GeometryFeature geometryFeature) : base(geometryFeature) { }
        public OrthodromeGraphic(Geometry? geometry) : base(geometry) { }

        public override GraphicType Type => GraphicType.Orthodrome;

        public void AddLinearPoint(Coordinate worldCoordinate)
        {
            if (_orthodrome is null)
                return;

            var last = _orthodrome;           
                
            while(last.Next != null)
            {
                last = last.Next;
            }
            last.Next = new Orthodrome(last.End, worldCoordinate.ToGeoPoint());
            _coordinates.Add(worldCoordinate);
            Geometry = RenderGeomerty(_coordinates);
        }

        public void AddRangeLinearPoints(IEnumerable<Coordinate> worldCoordinates)
        {
            if (_orthodrome is null)
                return;
                
            var last = _orthodrome;
            
            while(last.Next != null)
                last = last.Next;                
            foreach (var point in worldCoordinates)
            {
                last.Next = new Orthodrome(last.End, point.ToGeoPoint());
                _coordinates.Add(point);
                last = last.Next;
            }
            Geometry = RenderGeomerty(_coordinates);
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