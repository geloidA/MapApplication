using System.Collections.Generic;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using NetTopologySuite.Geometries;

namespace map_app.Services
{
    public static class GridReference
    {
        private static readonly Color LineLayerColor = new Color(150, 150, 150, 140);
        public static readonly MemoryLayer Grid;

        static GridReference()
        {
            Grid = CreateCoordinatesLayer("GridReference");
        }

        private static MemoryLayer CreateCoordinatesLayer(string layerName)
        {
            var features = CreateGridLines(10);
            features.AddRange(CreateGridLines(1));
            return new MemoryLayer(layerName)
            {
                Features = features,
                Style = new VectorStyle 
                {
                    Fill = new Brush(LineLayerColor),
                    Line = new Pen(LineLayerColor, 2),
                    Outline = new Pen(LineLayerColor, 3)
                }
            };
        }

        private static List<IFeature> CreateGridLines(int step)
        {
            var features = new List<IFeature>();

            for (int i = -180; i < 180; i += step)
            {
                var point1 = SphericalMercator.FromLonLat(i, 85);
                var point2 = SphericalMercator.FromLonLat(i, -85);
                var feature = new GeometryFeature { Geometry = new LineString(new[] { new Coordinate(point1.x, point1.y), new Coordinate(point2.x, point2.y) }) };
                features.Add(feature);
            }

            for (int i = -85; i < 85; i += step)
            {
                var point1 = SphericalMercator.FromLonLat(-180, i);
                var point2 = SphericalMercator.FromLonLat(180, i);
                var feature = new GeometryFeature { Geometry = new LineString(new[] { new Coordinate(point1.x, point1.y), new Coordinate(point2.x, point2.y) }) };
                features.Add(feature);
            }

            return features;
        }
        
    }
}