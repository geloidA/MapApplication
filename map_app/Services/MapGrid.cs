using System;
using System.Collections.Generic;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using NetTopologySuite.Geometries;

namespace map_app.Services
{
    public class MapGrid : WritableLayer
    {
        private const double LatKmInOneDegree = 111.319;
        private const double LonKmInOneDegree = 111.134;

        public double KilometerInterval { get; set; }
        public Color LineColor { get; set; }

        public MapGrid(double kilometerInterval, Color lineColor)
        {
            LineColor = lineColor;
            KilometerInterval = kilometerInterval;
            AddRange(GetGridLines(100));
            Style = new VectorStyle
            {
                Fill = null,
                Outline = null,
                Line = new Pen { Color = LineColor, Width = 2, PenStyle = PenStyle.Solid }
            };
        }

        public void RerenderLines(double kilometerInterval)
        {
            Clear();
            AddRange(GetGridLines(kilometerInterval));
        }

        private static IEnumerable<IFeature> GetGridLines(double kilometerInterval)
        {
            var step = Math.Round(kilometerInterval / LonKmInOneDegree, 4);
            
            for (var i = -180.0; i < 180.0; i += step)
            {
                var point1 = SphericalMercator.FromLonLat(i, 85);
                var point2 = SphericalMercator.FromLonLat(i, -85);
                yield return new GeometryFeature { Geometry = new LineString(new[] { new Coordinate(point1.x, point1.y), new Coordinate(point2.x, point2.y) }) };
            }

            step = Math.Round(kilometerInterval / LatKmInOneDegree, 4);

            for (var i = -85.0; i < 85.0; i += step)
            {
                var point1 = SphericalMercator.FromLonLat(-180, i);
                var point2 = SphericalMercator.FromLonLat(180, i);
                yield return new GeometryFeature { Geometry = new LineString(new[] { new Coordinate(point1.x, point1.y), new Coordinate(point2.x, point2.y) }) };
            }
        }        
    }
}