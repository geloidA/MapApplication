using BruTile.Predefined;
using BruTile.Web;
using map_app.Services.Layers;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling.Layers;
using Mapsui.UI;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Linq;

namespace map_app.Services;

public class MapCreator
{
    private static readonly Color EditModeColor = new(124, 22, 111, 180);

    public static Map Create()
    {
        var map = new Map
        {
            CRS = "EPSG:3857",
            Limiter = new ViewportLimiterKeepWithin
            {
                PanLimits = GetLimitsOfWorld(),
                ZoomLimits = new MinMax(2.5, 25000)
            }
        };
        var sources = App.Configuration.TileSources?
            .Select(CreateOwnTileLayer);        
        foreach (var layer in sources ?? Enumerable.Empty<ILayer>())
            map.Layers.Add(layer);
        var scaleWidget = GetScaleWidget(map);
        map.Widgets.Add(scaleWidget);
        map.Layers.Add(CreateTargetWritableLayer());
        return map;
    }

    private static IWidget GetScaleWidget(Mapsui.Map map)
    {
        return new ScaleBarWidget(map)
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            MarginX = 150f,
            MarginY = 25f
        };
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
        return new StyleCollection
        {
            Styles = new System.Collections.ObjectModel.Collection<IStyle>
            {
                new VectorStyle
                {
                    Fill = null,
                    Outline = new Pen(EditModeColor, 3),
                    Line = new Pen(EditModeColor, 3)
                }
            }
        };
    }

    private static TileLayer CreateOwnTileLayer(TileSource tileSource)
    {
        return new TileLayer(new HttpTileSource(new GlobalSphericalMercator(), tileSource.HttpTileSource))
        {
            Tag = new ManagedLayerTag
            {
                Name = tileSource.Name,
                HaveTileSource = true,
                CanRemove = true
            },
            Opacity = tileSource.Opacity,
            Attribution = new Hyperlink { Url = tileSource.HttpTileSource }
        };
    }
}