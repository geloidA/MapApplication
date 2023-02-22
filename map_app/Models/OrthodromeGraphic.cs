using System.Collections.Generic;
using map_app.Editing.Extensions;
using map_app.Models.Extensions;
using System.Linq;
using Mapsui.Nts;
using NetTopologySuite.Geometries;
using System;

namespace map_app.Models
{
    public class OrthodromeGraphic : BaseGraphic, IHoveringGraphic
    {
        private LinkedList<Orthodrome> _orthodromes;

        private OrthodromeGraphic(OrthodromeGraphic source) : base(source)
        {
            _orthodromes = source._orthodromes;
            HoverVertex = source.HoverVertex;
        }

        public Coordinate? HoverVertex { get; set; }

        public OrthodromeGraphic() : base()
        {
            _orthodromes = new LinkedList<Orthodrome>();
        }

        public OrthodromeGraphic(Coordinate startPoint) : base()
        {
            _coordinates = new List<Coordinate> { startPoint };
            _orthodromes = new LinkedList<Orthodrome>();
            _orthodromes.AddFirst(new Orthodrome(startPoint.ToGeoPoint(), startPoint.ToGeoPoint()));
        }

        public OrthodromeGraphic(List<Coordinate> points) : base()
        {
            if (points.Count < 1)
                throw new ArgumentException("Coordinate count can't be less 1");
            _coordinates = points;
            _orthodromes = CreateOrhodromes(_coordinates
                .ToGeoPoints()
                .ToList());
            Geometry = RenderGeometry();
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
            _coordinates.Add(worldCoordinate.Copy());
            Geometry = RenderGeometry();
        }

        public override OrthodromeGraphic Copy()
        {
            return new OrthodromeGraphic(this);
        }

        protected override Geometry RenderGeometry()
        {
            var result = new List<GeoPoint>();
            _orthodromes.Last!.Value.End = HoverVertex?.ToGeoPoint() ?? _orthodromes.Last.Value.End; // change last orthodrome point while mouse moving
            _orthodromes.Last!.Value.RenderPath();
            foreach (var orthodrome in _orthodromes)
                result.AddRange(orthodrome.Path);
            return new LineString(result.ToWorldPositions().ToArray());
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