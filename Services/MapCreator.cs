using BruTile.Predefined;
using BruTile.Web;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Layers.AnimatedLayers;
using Mapsui.Nts;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling.Layers;
using NetTopologySuite.IO;
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
        private static readonly Color PolygonLayerColor = new Color(20, 20, 20, 240);

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
            map.Layers.Add(new TileLayer(KnownTileSources.Create(KnownTileSource.OpenStreetMap)));
            //map.Layers.Add(CreateAnimatedAircraftsLayer());
            map.Layers.Add(CreatePointLayer());
            map.Layers.Add(CreateLineLayer());
            map.Layers.Add(CreatePolygonLayer());
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

        private static WritableLayer CreatePolygonLayer()
        {
            var polygonLayer = new WritableLayer
            {
                Name = "Layer 3",
                Style = CreatePolygonStyle()
            };

            var wkt = "POLYGON ((1261416.17275404 5360656.05714234, 1261367.50386493 5360614.2556425, 1261353.47050427 5360599.62511755, 1261338.83997932 5360576.03712836, 1261337.34706862 5360570.6626498, 1261375.8641649 5360511.2448036, 1261383.92588273 5360483.17808227, 1261391.98760055 5360485.56673941, 1261393.48051126 5360480.490843, 1261411.99260405 5360487.6568144, 1261430.50469684 5360496.9128608, 1261450.21111819 5360507.06465361, 1261472.00761454 5360525.5767464, 1261488.13105019 5360544.98458561, 1261488.1310502 5360545.28316775, 1261481.26366093 5360549.76189988, 1261489.6239609 5360560.21227484, 1261495.59560374 5360555.13637843, 1261512.91336796 5360573.05130694, 1261535.00844645 5360598.43078898, 1261540.08434286 5360619.03295677, 1261535.90419287 5360621.12303176, 1261526.64814648 5360623.21310675, 1261489.32537876 5360644.41243881, 1261458.27283602 5360661.73020303, 1261438.26783253 5360662.02878517, 1261427.22029328 5360660.23729232, 1261416.17275404 5360656.05714234))";
            var polygon = new WKTReader().Read(wkt);
            IFeature feature = new GeometryFeature { Geometry = polygon };
            polygonLayer.Add(feature);

            return polygonLayer;
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
        
        private static IStyle CreatePolygonStyle()
        {
            return new VectorStyle
            {
                Fill = new Brush(new Color(PolygonLayerColor)),
                Line = new Pen(PolygonLayerColor, 3),
                Outline = new Pen(PolygonLayerColor, 3)
            };
        }
    
        private HttpTileSource CreateOwnTileSourse()
        {
            return new HttpTileSource(new GlobalSphericalMercator(), "http://10.5.23.21/tile/{z}/{x}/{y}.png");
        }
    }
}