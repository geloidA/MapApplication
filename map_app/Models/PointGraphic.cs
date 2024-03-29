using map_app.Services.Attributes;
using Mapsui.Nts;
using Mapsui.Nts.Extensions;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace map_app.Models
{
    [Label("Точка")]
    public class PointGraphic : BaseGraphic
    {
        private SymbolStyle _style = new();
        private double _scale = 1;

        [JsonProperty]
        public string? Image { get; set; }

        [JsonProperty]
        public double Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                _style.SymbolScale = _scale;
            }
        }

        private PointGraphic(PointGraphic source) : base(source)
        {
            Image = source.Image;
            Scale = source.Scale;
        }

        public override GraphicType Type => GraphicType.Point;

        public PointGraphic() : base() { }

        public PointGraphic(Coordinate point) : this(new List<Coordinate> { point })
        {
        }

        public PointGraphic(List<Coordinate> points) : this()
        {
            if (points.Count != 1)
                throw new ArgumentException($"List need contain one point, but was {points.Count}");
            _coordinates = points;
            RerenderGeometry();
        }

        public PointGraphic(GeometryFeature geometryFeature) : base(geometryFeature) { }

        public PointGraphic(Geometry? geometry) : base(geometry) { }

        public override VectorStyle GraphicStyle
        {
            get => _style;
            set
            {
                if (value is not SymbolStyle smblStyle)
                    throw new ArgumentException("PointGraphic's style only can be SymbolStyle");
                Styles.Remove(_style);
                _style = smblStyle;
                Styles.Add(_style);
            }
        }

        protected override Geometry RenderGeometry() => _coordinates[0].ToPoint();

        public override PointGraphic Copy() => new(this);

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
            GC.SuppressFinalize(this);
        }
    }
}