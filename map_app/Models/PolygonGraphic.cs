using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Nts;
using NetTopologySuite.Geometries;

namespace map_app.Models
{
    public class PolygonGraphic : BaseGraphic, IStepByStepGraphic
    {
        private Coordinate? _hoverVertex;

        private PolygonGraphic(PolygonGraphic source) : base(source) { }

        public PolygonGraphic() : base() { }

        public PolygonGraphic(List<Coordinate> points) : base(points)
        {
            if (points.Count < 2)
                throw new ArgumentException("Points length can't be less then 2");
            Geometry = RenderGeometry();
        }

        public PolygonGraphic(GeometryFeature geometryFeature) : base(geometryFeature) { }
        public PolygonGraphic(Geometry? geometry) : base(geometry) { }

        public override GraphicType Type => GraphicType.Polygon;

        public void AddPoint(Coordinate worldCoordinate)
        {
            _coordinates.Add(worldCoordinate);
            _hoverVertex = worldCoordinate;
            Geometry = RenderGeometry();
        }

        public void AddRangePoints(IEnumerable<Coordinate> worldPoints)
        {
            foreach(var point in worldPoints)
                _coordinates.Add(point);                
            Geometry = RenderGeometry();
        }

        public override PolygonGraphic LightCopy()
        {
            return new PolygonGraphic(this);
        }

        protected override Geometry RenderGeometry()
        {
            var linearRing = _coordinates.ToList();
            linearRing.Add(_coordinates[0].Copy()); // Add first coordinate at end to close the ring.
            return new Polygon(new LinearRing(linearRing.ToArray()));
        }

        public void RemoveHoverVertex()
        {
            if (_hoverVertex is null) return;
            if (!ReferenceEquals(_hoverVertex, _coordinates[_coordinates.Count - 1]))
                return;
            _coordinates.RemoveAt(_coordinates.Count - 1);
            _hoverVertex = null;
        }
    }
}