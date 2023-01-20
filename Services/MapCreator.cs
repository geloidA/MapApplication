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
using map_app.Editing.Layers;
using map_app.Network;

namespace map_app.Services
{
    public class MapCreator
    {
        private static readonly Color EditModeColor = new Color(124, 22, 111, 180);

        private static readonly SymbolStyle? DisableStyle = new SymbolStyle { Enabled = false };

        public static Map Create()
        {
            var map = new Map();
            map.Layers.Add(new TileLayer(DictKnownTileSources.Create("www.openstreetmap.org")){ Name = "UserMain"});
            //map.Layers.Add(CreateAnimatedAircraftsLayer());
            map.Layers.Add(CreateTargetWritableLayer());
            var editLayer = CreateEditLayer();
            map.Layers.Add(editLayer);
            map.Layers.Add(new VertexOnlyLayer(editLayer) { Name = "VertexLayer" });            
            return map;
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

        private static WritableLayer CreateEditLayer()
        {
            return new WritableLayer
            {
                Name = "EditLayer",
                Style = CreateEditLayerStyle(),
                IsMapInfoLayer = true
            };
        }

        private static IStyle CreateEditLayerStyle()
        {
            return new VectorStyle
            {
                Fill = new Brush(EditModeColor),
                Line = new Pen(Color.Red, 3),
                Outline = new Pen(Color.Red, 3)
            };
        }

        private static WritableLayer CreateTargetWritableLayer()
        {
            var layer = new OwnWritableLayer
            {
                Name = "Target Layer",
                Style = CreateTargetLayerStyle()
            };
            return layer;
        }

        private static IStyle CreateTargetLayerStyle()
        {
            return new VectorStyle
            {
                Fill = new Brush(EditModeColor)
            };
        }
    
        private HttpTileSource CreateOwnTileSourse()
        {
            return new HttpTileSource(new GlobalSphericalMercator(), "http://10.5.23.21/tile/{z}/{x}/{y}.png");
        }
    }
}