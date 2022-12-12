using Avalonia.Controls;
using Mapsui.UI.Avalonia;
using Mapsui.Tiling.Layers;
using BruTile.Web;
using Mapsui;
using Mapsui.Extensions;
using BruTile.Predefined;
using Mapsui.Layers;
using Mapsui.Styles.Thematics;
using Mapsui.Styles;
using Mapsui.Layers.AnimatedLayers;

namespace map_app;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var map = this.FindControl<MapControl>("MapControl");
        
        map.Map?.Layers.Add(new TileLayer(KnownTileSources.Create(KnownTileSource.OpenStreetMap)));
        map.Map?.Layers.Add(CreateAnimatedAircraftsLayer());
    }

    private ILayer CreateAnimatedAircraftsLayer()
    {
        return new AnimatedPointLayer(new AnimationAircraftsProvider())
        {
            Name = "Aircrafts",
            Style = CreatePointStyle()
        };
    }

    private IStyle CreatePointStyle()
    {
        return new ThemeStyle(f => 
        {
            return CreateSvgArrowStyle("Resources.Assets.aircraft.png", 0.1, f);
        });
    }

    private IStyle CreateSvgArrowStyle(string embeddedResourcePath, double scale, IFeature feature)
    {
        var bitmapId = typeof(MainWindow).LoadBitmapId(embeddedResourcePath);
        return new SymbolStyle
        {
            BitmapId = bitmapId,
            SymbolScale = scale,
            SymbolOffset = new RelativeOffset(0.0, 0.5),
            SymbolRotation = (double)feature["rotation"]!
        };
    }

    private HttpTileSource CreateOwnTileSourse()
    {
        return new HttpTileSource(new GlobalSphericalMercator(), "http://10.5.23.21/tile/{z}/{x}/{y}.png");
    }
}