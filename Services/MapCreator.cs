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
        private static readonly Color PointLayerColor = new Color(240, 240, 240, 240);
        private static readonly Color LineLayerColor = new Color(150, 150, 150, 240);

        private static readonly SymbolStyle? SelectedStyle = new SymbolStyle
        {
            Fill = new Brush(new Color(0, 240, 0, 180)),
            Outline = new Pen(Color.Red, 3),
            Line = new Pen(Color.Red, 3)
        };

        private static readonly SymbolStyle? DisableStyle = new SymbolStyle { Enabled = false };

        public static Map Create()
        {
            var map = new Map();
            map.Layers.Add(new TileLayer(DictKnownTileSources.Create("www.openstreetmap.org")){ Name = "UserMain"});
            //map.Layers.Add(CreateAnimatedAircraftsLayer());
            map.Layers.Add(CreatePointLayer());
            map.Layers.Add(CreateLineLayer());
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
            var bitmapId = typeof(MainWindow).LoadBitmapId(embeddedResourcePath);
            return new SymbolStyle
            {
                BitmapId = bitmapId,
                SymbolScale = scale,
                SymbolOffset = new RelativeOffset(0.0, 0.5),
                SymbolRotation = (double)feature["rotation"]!
            };
        }

        private static WritableLayer CreatePointLayer()
        {
            return new WritableLayer
            {
                Name = "Layer 1",
                Style = CreatePointStyle()
            };
        }

        private static IStyle CreatePointStyle()
        {
            return new VectorStyle
            {
                Fill = new Brush(PointLayerColor),
                Line = new Pen(PointLayerColor, 3),
                Outline = new Pen(Color.Gray, 2)
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

        private static StyleCollection CreateEditLayerStyle()
        {
            return new StyleCollection
            {
                CreateEditLayerBasicStyle(),
                CreateSelectedStyle()
            };
        }

        private static IStyle CreateEditLayerBasicStyle()
        {
            var editStyle = new VectorStyle
            {
                Fill = new Brush(EditModeColor),
                Line = new Pen(EditModeColor, 3),
                Outline = new Pen(EditModeColor, 3)
            };
            return editStyle;
        }

        private static IStyle CreateSelectedStyle()
        {
            // To show the selected style a ThemeStyle is used which switches on and off the SelectedStyle
            // depending on a "Selected" attribute.
            return new ThemeStyle(f =>
            {
                return (bool?)f["Selected"] == true ? SelectedStyle : DisableStyle;
            });
        }

        private static WritableLayer CreateLineLayer()
        {
            var lineLayer = new WritableLayer
            {
                Name = "Layer 2",
                Style = CreateLineStyle()
            };

            // todo: add data

            return lineLayer;
        }

        private static WritableLayer CreateTargetWritableLayer()
        {
            var layer = new WritableLayer
            {
                Name = "Target Layer",
                Style = CreateLineStyle()
            };
            return layer;
        }

        private static IStyle CreateLineStyle()
        {
            return new VectorStyle
            {
                Fill = new Brush(LineLayerColor),
                Line = new Pen(LineLayerColor, 2),
                Outline = new Pen(LineLayerColor, 3)
            };
        }
    
        private HttpTileSource CreateOwnTileSourse()
        {
            return new HttpTileSource(new GlobalSphericalMercator(), "http://10.5.23.21/tile/{z}/{x}/{y}.png");
        }
    }
}