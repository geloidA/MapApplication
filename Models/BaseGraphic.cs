using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Nts;
using Newtonsoft.Json;
using Mapsui.Styles;
using map_app.Editing.Extensions;
using NetTopologySuite.Geometries;
using map_app.Models.Extensions;

namespace map_app.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class BaseGraphic : GeometryFeature
    {
        private Color? _color;
        private double _opacity;
        private VectorStyle? _style;
        protected List<Coordinate> _coordinates = new();

        protected BaseGraphic(BaseGraphic source) 
        {
            _color = source._color;
            _opacity = source._opacity;
            UserTags = source.UserTags;
            _style = source._style;
            Styles = source.Styles;
            _coordinates = source._coordinates;
            Geometry = source.Geometry;
        }

        public BaseGraphic(List<LinearPoint> points) : base()
        {
            _coordinates = points.ToCoordinates().ToList();
            InitializeGraphicStyle();
        }

        public BaseGraphic(List<Coordinate> points) : base() 
        {
            _coordinates = points;
            InitializeGraphicStyle();
        }

        public BaseGraphic(GeometryFeature geometryFeature) : base(geometryFeature) { InitializeGraphicStyle(); }
        public BaseGraphic(Geometry? geometry) : base(geometry) { InitializeGraphicStyle(); }

        [JsonProperty]
        public abstract GraphicType Type { get; }

        [JsonProperty]
        public Dictionary<string, string>? UserTags { get; set; }

        [JsonProperty]
        public Color? Color 
        {
            get => _color;
            set
            {
                if (value is null)
                    throw new NullReferenceException();
                _color = value;
                _style!.Fill = new Brush(value);
                _style!.Line = new Pen(value, 2);
            }
        }

        [JsonProperty]
        public double Opacity
        {
            get => _opacity;
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentOutOfRangeException();
                _opacity = value;
                _style!.Opacity = (float)_opacity;
            }
        }

        [JsonProperty]
        public IReadOnlyList<GeoPoint> GeoPoints => _coordinates.Select(x => x.ToGeoPoint()).ToList();

        [JsonProperty]
        public IReadOnlyList<LinearPoint> LinearPoints => _coordinates.Select(x => x.ToLinearPoint()).ToList();

        /// <summary>
        /// Recalculation Geometry property when set method is called
        /// </summary>
        public virtual IReadOnlyList<Coordinate> Coordinates
        {
            get => _coordinates;
            set
            {
                if (value is null)
                    throw new ArgumentNullException();
                
                _coordinates = value.ToList();
                Geometry = RenderGeometry();
            }
        }
        
        public new Geometry? Geometry
        {
            get => base.Geometry;
            protected set => base.Geometry = value;
        }

        protected abstract Geometry RenderGeometry();

        public abstract BaseGraphic LightCopy();
        
        public void RerenderGeometry()
        {
            Geometry = RenderGeometry();
        }

        private void InitializeGraphicStyle()
        {
            _style = new VectorStyle();
            Styles.Add(_style);
        }
    }
}