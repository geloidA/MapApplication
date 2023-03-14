using System;
using System.Collections.Generic;
using System.Linq;
using map_app.Services.Attributes;
using Mapsui.Nts;
using NetTopologySuite.Geometries;

namespace map_app.Models
{
    [Label("Полигон")]
    public class PolygonGraphic : BaseGraphic, IHoveringGraphic
    {
        private PolygonGraphic(PolygonGraphic source) : base(source) { }

        public Coordinate? HoverVertex { get; set; }

        public PolygonGraphic() : base() { }

        public PolygonGraphic(Coordinate startPoint) : base()
        {
            _coordinates = new List<Coordinate> { startPoint };
        }

        public PolygonGraphic(List<Coordinate> points) : this()
        {
            if (points.Count < 2)
                throw new ArgumentException("Points length can't be less then 2");
            _coordinates = points;
            Geometry = RenderGeometry();
        }

        public PolygonGraphic(GeometryFeature geometryFeature) : base(geometryFeature) { }
        public PolygonGraphic(Geometry? geometry) : base(geometry) { }

        public override GraphicType Type => GraphicType.Polygon;

        public void AddPoint(Coordinate worldCoordinate)
        {
            _coordinates.Add(worldCoordinate);
            Geometry = RenderGeometry();
        }

        public override PolygonGraphic Copy() => new PolygonGraphic(this);

        protected override Geometry RenderGeometry()
        {
            var linearRing = _coordinates.ToList();
            if (HoverVertex is not null)
                linearRing.Add(HoverVertex);
            linearRing.Add(_coordinates[0].Copy()); // Add first coordinate at end to close the ring.
            return new Polygon(new LinearRing(linearRing.ToArray()));
        }
    }
}