using System;
using System.Collections.Generic;
using System.IO;
using Mapsui.Nts;
using Mapsui.Nts.Extensions;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace map_app.Models
{
    public class PointGraphic : BaseGraphic
    {
        [JsonProperty]
        public string? Image { get; set; }

        private PointGraphic(PointGraphic source) : base(source)
        {
            Image = source.Image;
        }

        public override GraphicType Type => GraphicType.Point;

        public PointGraphic() : base() { }
        public PointGraphic(List<Coordinate> points) : base(points) { Geometry = RenderGeometry(); }
        public PointGraphic(GeometryFeature geometryFeature) : base(geometryFeature) { }
        public PointGraphic(Geometry? geometry) : base(geometry) { }

        protected override Geometry RenderGeometry()
        {
            if (_coordinates.Count != 1)
                throw new ArgumentException($"List need contain one point, but was {_coordinates.Count}");
            var coordinate = _coordinates[0] ?? throw new NullReferenceException("Point was null");
            return coordinate.ToPoint();
        }

        public override PointGraphic LightCopy()
        {
            return new PointGraphic(this);
        }

        public override void Dispose()
        {
            base.Dispose();
            if (Image != null)
            {
                if (BitmapRegistry.Instance.TryGetBitmapId(Image, out int bitmapId))
                {
                    var bitmap = BitmapRegistry.Instance.Unregister(bitmapId);
                    (bitmap as IDisposable)?.Dispose();
                }
            }
        }
    }
}