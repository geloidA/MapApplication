using BruTile.Predefined;
using BruTile.Web;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Layers.AnimatedLayers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling.Layers;
using map_app.Views;
using map_app.Network;
using Mapsui.Projections;
using Mapsui.UI;
using Mapsui.Widgets;
using map_app.Services.Renders;

namespace map_app.Services;

public class MapCreator
{
    private static readonly Color EditModeColor = new Color(124, 22, 111, 180);

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
        var scaleWidget = new ScalePeaceWidget(map);
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

    private static ILayer CreateAnimatedAircraftsLayer()
    {
        return new AnimatedPointLayer(new AnimationAircraftsProvider())
        {
            Name = "Aircrafts",
            Style = CreateAircraftPointStyle()
        };
    }

    private static IStyle CreateAircraftPointStyle()
    {
        return new ThemeStyle(f => 
        {
            return CreateSvgArrowStyle("Resources.Assets.aircraft.png", 0.1, f);
        });
    }

    private static IStyle CreateSvgArrowStyle(string embeddedResourcePath, double scale, IFeature feature)
    {
        var bitmapId = typeof(MainView).LoadBitmapId(embeddedResourcePath);
        return new SymbolStyle
        {
            BitmapId = bitmapId,
            SymbolScale = scale,
            SymbolOffset = new RelativeOffset(0.0, 0.5),
            SymbolRotation = (double)feature["rotation"]!
        };
    }

    private static ILayer CreateTargetWritableLayer()
    {
        return new GraphicsLayer
        {
            Name = "Graphic Layer",
            IsMapInfoLayer = true,
            Tag = "Graphic",
            Style = CreateTargetLayerStyle()
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