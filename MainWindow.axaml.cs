using Avalonia.Controls;
using System;
using Mapsui.UI.Avalonia;
using Mapsui.Tiling.Layers;
using BruTile.Web;
using Mapsui;
using Mapsui.Extensions;
using System.Reflection;
using Mapsui.Utilities;
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
        
        map.Map?.Layers.Add(new TileLayer(KnownTileSources.Create(KnownTileSource.BingAerial)));
        var layer = new TileLayer(CreateTileSourse());
        layer.Opacity = 0.5;
        map.Map?.Layers.Add(layer);
        map.Map?.Layers.Add(CreateAnimatedPointLayer());
    }

    private ILayer CreateAnimatedPointLayer()
    {
        return new AnimatedPointLayer(new AnimationPlaneProvider())
        {
            Name = "Plane",
            Style = CreatePointStyle()
        };
    }

    private IStyle CreatePointStyle()
    {
        return new ThemeStyle(f => 
        {
            return CreateSvgArrowStyle("Resources.Assets.plane.svg", 0.2, f);
        });
    }

    private IStyle CreateSvgArrowStyle(string embeddedResourcePath, double scale, IFeature feature)
    {
        var bitmapId = LoadSvgId(typeof(MainWindow), embeddedResourcePath);
        return new SymbolStyle
        {
            BitmapId = bitmapId,
            SymbolScale = scale,
            SymbolOffset = new RelativeOffset(0.0, 0.5),
            SymbolRotation = (double)feature["rotation"]!
        };
    }

    private int LoadSvgId(Type typeInAssemblyOfEmbeddedResource, string relativePathToEmbeddedResource)
    {
        var assembly = typeInAssemblyOfEmbeddedResource.GetTypeInfo().Assembly;
        var fullName = assembly.GetFullName(relativePathToEmbeddedResource);
            if (!BitmapRegistry.Instance.TryGetBitmapId(fullName, out var bitmapId))
            {
                var resourses = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                var result = assembly.GetManifestResourceStream(fullName).LoadSvgPicture();
                if (result != null)
                {
                    bitmapId = BitmapRegistry.Instance.Register(result, fullName);
                    return bitmapId;    
                }
            }

        return bitmapId;
    }

    private HttpTileSource CreateTileSourse()
    {
        return new HttpTileSource(new GlobalSphericalMercator(), "http://10.5.23.21/tile/{z}/{x}/{y}.png");
    }
}