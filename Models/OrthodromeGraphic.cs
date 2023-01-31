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

        private OrthodromeGraphic(OrthodromeGraphic source) : base(source)
        {
            _orthodrome = source._orthodrome;
        }

        public OrthodromeGraphic(List<Coordinate> points) : base(points)
        {
            var firstPoints = points.Take(2).ToArray();
            if (firstPoints.Length < 2)
                throw new ArgumentException("length can't be less 2");

            _orthodrome = new Orthodrome(firstPoints[0].ToGeoPoint(), firstPoints[1].ToGeoPoint());
            AddRangePoints(points.Skip(2));
        }

        public OrthodromeGraphic(GeometryFeature geometryFeature) : base(geometryFeature) { }
        public OrthodromeGraphic(Geometry? geometry) : base(geometry) { }

        public override GraphicType Type => GraphicType.Orthodrome;

        public override IReadOnlyList<Coordinate> Coordinates 
        { 
            get => base.Coordinates; 
            set 
            {
                _orthodrome = Orthodrome.Create(value.ToGeoPoints().ToList());
                base.Coordinates = value;
            } 
        }

        public void AddPoint(Coordinate worldCoordinate)
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
            Geometry = RenderGeometry();
        }

        public void AddRangePoints(IEnumerable<Coordinate> worldCoordinates)
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
            Geometry = RenderGeometry();
        }

        public override OrthodromeGraphic LightCopy()
        {
            return new OrthodromeGraphic(this);
        }

        public Geometry RenderStepGeometry(Coordinate worldPosition)
        {
            throw new System.NotImplementedException();
        }

        protected override Geometry RenderGeometry()
        {
            var last = _coordinates.Last();
            var result = new List<GeoPoint>();
            var next = _orthodrome;
            while(next != null)
            {
                if (next.Next is null) // change last orthodrome point while mouse moving
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