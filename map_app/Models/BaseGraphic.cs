using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Nts;
using Newtonsoft.Json;
using Mapsui.Styles;
using map_app.Editing.Extensions;
using NetTopologySuite.Geometries;
using map_app.Services.Renders;
using map_app.Services.IO;

namespace map_app.Models;

[JsonObject(MemberSerialization.OptIn)]
[JsonConverter(typeof(BaseGraphicConverter))]
public abstract class BaseGraphic : GeometryFeature
{
    private Color _color = new(Color.Black);
    private double _opacity = 1;
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

    public BaseGraphic() => InitializeStyles();

    public BaseGraphic(GeometryFeature geometryFeature) : base(geometryFeature) => InitializeStyles();

    public BaseGraphic(Geometry? geometry) : base(geometry) => InitializeStyles();

    private void InitializeStyles()
    {
        Styles.Add(GraphicStyle);
        if (this.GetType() != typeof(PointGraphic))
            Styles.Add(new LabelDistanceStyle { Enabled = false });
    }

    [JsonProperty]
    public string? Name { get; set; }

    [JsonProperty]
    public abstract GraphicType Type { get; }

    public virtual VectorStyle GraphicStyle { get; set; } = new();

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
    public IEnumerable<GeoPoint> GeoPoints => _coordinates.Select(x => x.ToGeoPoint());

    [JsonProperty]
    public IEnumerable<LinearPoint> LinearPoints => _coordinates.Select(x => x.ToLinearPoint());

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
            RerenderGeometry();
        }
    }
    
    public new Geometry? Geometry
    {
        get => base.Geometry;
        private set => base.Geometry = value;
    }

    protected abstract Geometry RenderGeometry();

    public abstract BaseGraphic Copy();
    
    public void RerenderGeometry() => Geometry = RenderGeometry();
}