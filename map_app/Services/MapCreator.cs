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
        var layer = new TileLayer(MyTileSource.Create("www.openstreetmap.org"))
        { 
            Name = "Основной",
            Tag = "User"
        };
        var scaleWidget = GetScaleWidget(map);
        map.Widgets.Add(scaleWidget);
        map.Layers.Add(layer);
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
            Tag = "Graphic",
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

    private HttpTileSource CreateOwnTileSourse()
    {
        return new HttpTileSource(new GlobalSphericalMercator(), "http://10.5.23.21/tile/{z}/{x}/{y}.png");
    }
}