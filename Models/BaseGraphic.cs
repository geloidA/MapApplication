using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Nts;
using Newtonsoft.Json;
using Mapsui.Styles;
using map_app.Editing.Extensions;
using NetTopologySuite.Geometries;
using ReactiveUI.Fody.Helpers;
using Mapsui;

namespace map_app.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class BaseGraphic : GeometryFeature
    {
        protected List<Coordinate> _coordinates = new();

        public BaseGraphic(List<Coordinate> points) : base() 
        { 
            _coordinates = points;
            Geometry = RenderGeomerty(points);
        }

        public BaseGraphic(GeometryFeature geometryFeature) : base(geometryFeature) { }
        public BaseGraphic(Geometry? geometry) : base(geometry) { }

        [JsonProperty]
        public abstract GraphicType Type { get; }

        [JsonProperty]
        public Dictionary<string, IUserTag>? UserTags { get; set; }

        [JsonProperty]
        public Color? Color { get; set; } // todo: auto generate style when change color

        [JsonProperty]
        public double Opacity { get; set; }

        [JsonProperty]
        public IReadOnlyList<GeoPoint> GeoPoints => _coordinates.Select(x => x.ToGeoPoint()).ToList();

        [JsonProperty]
        public IReadOnlyList<LinearPoint> LinearPoints => _coordinates.Select(x => x.ToLinearPoint()).ToList();

        /// <summary>
        /// Recalculation Geometry property when set method is called
        /// </summary>
        public IReadOnlyList<Coordinate> Coordinates
        {
            get => _coordinates;
            set
            {
                if (value is null)
                    throw new ArgumentNullException();
                
                _coordinates = value.ToList();
                Geometry = RenderGeomerty(_coordinates);
            }
        }
        
        public new Geometry? Geometry
        {
            get => base.Geometry;
            protected set => base.Geometry = value;
        }

        protected abstract Geometry RenderGeomerty(List<Coordinate> points);
        
        public void RerenderGeometry()
        {
            Geometry = RenderGeomerty(_coordinates);
        }
    }
}