using BruTile.Predefined;
using BruTile.Web;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Tiling.Layers;
using Mapsui.Projections;
using Mapsui.UI;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using map_app.Services.Layers;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Globalization;

namespace map_app.Services;

public class MapCreator
{
    private static readonly Color EditModeColor = new(124, 22, 111, 180);

    public static Map Create()
    {
        var map = new Map { CRS = "EPSG:3857" };
        map.Limiter = new ViewportLimiterKeepWithin
        {
            PanLimits = GetLimitsOfWorld(),
            ZoomLimits = new MinMax(2.5, 25000)
        };
        var sources = App.Config
            .GetSection("tile_sources")
            .GetChildren()
            .Select(x =>  new 
            { 
                Name = x["Name"], 
                Opacity = double.Parse(x["Opacity"]!, CultureInfo.InvariantCulture), 
                HttpTileSource = x["HttpTileSource"] 
            })
            .Select(x => CreateOwnTileLayer(x.Name!, x.Opacity, x.HttpTileSource!))
            .Reverse();
        foreach (var layer in sources)
            map.Layers.Add(layer);
        var scaleWidget = GetScaleWidget(map);
        map.Widgets.Add(scaleWidget);
        map.Layers.Add(CreateTargetWritableLayer());
        return map;
    }

    private static IWidget GetScaleWidget(Mapsui.Map map)
    {
        var scaleWidget = new ScaleBarWidget(map);
        scaleWidget.HorizontalAlignment = HorizontalAlignment.Right;
        scaleWidget.MarginX = 150f;
        scaleWidget.MarginY = 25f;
        return scaleWidget;
    }

    private static MRect GetLimitsOfWorld()
    {
        var (minX, minY) = SphericalMercator.FromLonLat(-180, 85);
        var (maxX, maxY) = SphericalMercator.FromLonLat(180, -85);
        return new MRect(minX, minY, maxX, maxY);
    }

    private static ILayer CreateTargetWritableLayer()
    {
        return new GraphicsLayer
        {
            Name = nameof(GraphicsLayer),
            IsMapInfoLayer = true,
            Tag = new ManagedLayerTag { Name = "Объекты" },
            Style = null
        };
    }

    private static IStyle CreateTargetLayerStyle()
    {
        var style = new StyleCollection();
        style.Styles = new System.Collections.ObjectModel.Collection<IStyle>
        {
            new VectorStyle
            {
                Fill = null,
                Outline = new Pen(EditModeColor, 3),
                Line = new Pen(EditModeColor, 3)
            }
        };
        return style;
    }

    private static TileLayer CreateOwnTileLayer(string name, double opacity, string httpTileSource)
    {
        return new TileLayer(new HttpTileSource(new GlobalSphericalMercator(), httpTileSource))
        {
            Tag = new ManagedLayerTag
            {
                Name = name,
                HaveTileSource = true,
                CanRemove = true
            },
            Opacity = opacity
        };
    }
}