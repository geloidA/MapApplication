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
        private LinkedList<Orthodrome> _orthodromes;
        private Coordinate? _hoverVertex;

        private OrthodromeGraphic(OrthodromeGraphic source) : base(source)
        {
            _orthodromes = source._orthodromes;
            _hoverVertex = source._hoverVertex;
        }

        public OrthodromeGraphic(List<Coordinate> points) : base(points)
        {
            var firstPoints = points.Take(2).ToArray();
            if (firstPoints.Length < 2)
                throw new ArgumentException("length can't be less 2");

            _orthodromes = new LinkedList<Orthodrome>();
            _orthodromes.AddFirst(new Orthodrome(firstPoints[0].ToGeoPoint(), firstPoints[1].ToGeoPoint()));
            AddRangePoints(points.Skip(2));
        }

        public OrthodromeGraphic(GeometryFeature geometryFeature) : base(geometryFeature) { _orthodromes = new LinkedList<Orthodrome>(); }
        public OrthodromeGraphic(Geometry? geometry) : base(geometry) { _orthodromes = new LinkedList<Orthodrome>(); }

        public override GraphicType Type => GraphicType.Orthodrome;

        public override IReadOnlyList<Coordinate> Coordinates 
        { 
            get => base.Coordinates; 
            set 
            {
                _orthodromes = CreateOrhodromes(value.ToGeoPoints().ToList());
                base.Coordinates = value;
            } 
        }

        public void AddPoint(Coordinate worldCoordinate)
        {
            _orthodromes.AddLast(new Orthodrome(_orthodromes.Last!.Value.End, worldCoordinate.ToGeoPoint()));            
            _coordinates.Add(worldCoordinate);
            _hoverVertex = worldCoordinate;
            Geometry = RenderGeometry();
        }

        public void AddRangePoints(IEnumerable<Coordinate> worldCoordinates)
        {            
            foreach (var point in worldCoordinates)
            {
                _orthodromes.AddLast(new Orthodrome(_orthodromes.Last!.Value.End, point.ToGeoPoint()));
                _coordinates.Add(point);
            }
            Geometry = RenderGeometry();
        }

        public override OrthodromeGraphic LightCopy()
        {
            return new OrthodromeGraphic(this);
        }

        protected override Geometry RenderGeometry()
        {
            var last = _coordinates.Last();
            var result = new List<GeoPoint>();
            _orthodromes.Last!.Value.End = last.ToGeoPoint(); // change last orthodrome point while mouse moving
            foreach (var orthodrome in _orthodromes)
                result.AddRange(orthodrome.Path);
            return new LineString(result.ToWorldPositions().ToArray());
        }

        public bool RemoveHoverVertex()
        {
            if (_hoverVertex is null)
                return false;
            if (!ReferenceEquals(_hoverVertex, _coordinates[_coordinates.Count - 1]))
                return false;
            _coordinates.RemoveAt(_coordinates.Count - 1);
            _hoverVertex = null;
            return true;
        }

        private static LinkedList<Orthodrome> CreateOrhodromes(List<GeoPoint> points)
        {
            if (points.Count < 2) throw new ArgumentException("Points count must be bigger then 2");
            var orthodromes = new LinkedList<Orthodrome>();
            orthodromes.AddFirst(new Orthodrome(points[0], points[1]));
            foreach (var point in points.Skip(2))
                orthodromes.AddLast(new Orthodrome(orthodromes.Last!.Value.End, point));
            return orthodromes;
        }
    }
}