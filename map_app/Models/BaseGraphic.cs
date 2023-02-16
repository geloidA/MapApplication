using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Nts;
using Newtonsoft.Json;
using Mapsui.Styles;
using map_app.Editing.Extensions;
using NetTopologySuite.Geometries;

namespace map_app.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class BaseGraphic : GeometryFeature
    {
        private Color _color;
        private double _opacity = 1;

        private VectorStyle _graphicStyle = new VectorStyle();
        protected List<Coordinate> _coordinates = new();

        protected BaseGraphic(BaseGraphic source) 
        {
            _color = source._color;
            _opacity = source._opacity;
            UserTags = source.UserTags;
            GraphicStyle = source.GraphicStyle;
            Styles = source.Styles;
            _coordinates = source._coordinates;
            Geometry = source.Geometry;
        }

        public BaseGraphic() : this(new List<Coordinate3D>()) { }

        public BaseGraphic(IEnumerable<Coordinate> points) : base() 
        {
            _coordinates = points.ToList();
            _color = new Color(Color.Black);            
            Styles.Add(GraphicStyle);
        }

        public BaseGraphic(GeometryFeature geometryFeature) : base(geometryFeature) 
        {
            Styles.Add(GraphicStyle);
            _color = new Color(Color.Black); 
        }
        
        public BaseGraphic(Geometry? geometry) : base(geometry) 
        {
            Styles.Add(GraphicStyle);
            _color = new Color(Color.Black);
        }

        [JsonProperty]
        public abstract GraphicType Type { get; }

        public virtual VectorStyle GraphicStyle 
        { 
            get => _graphicStyle; 
            set
            {
                Styles.Remove(_graphicStyle);
                _graphicStyle = value;
                Styles.Add(_graphicStyle);
            } 
        }

        [JsonProperty]
        public Dictionary<string, string>? UserTags { get; set; }

        [JsonProperty("Color")]
        public Color StyleColor 
        {
            get => _color;
            set
            {
                if (value is null)
                    throw new NullReferenceException();
                _color = value;
                GraphicStyle.Fill = new Brush(_color);
                GraphicStyle.Line = new Pen(_color, 2);
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
                GraphicStyle.Opacity = (float)_opacity;
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
    }
}