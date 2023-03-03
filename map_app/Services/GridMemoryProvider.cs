using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Providers;
using NetTopologySuite.Geometries;

namespace map_app.Services
{
    public class GridMemoryProvider : IProvider, IDynamic
    {
        private IReadOnlyViewport _viewport;
        private double _kilometerInterval;
        private bool _isActive;

        public event DataChangedEventHandler? DataChanged;

        public GridMemoryProvider(IReadOnlyViewport viewport, IObservable<bool> isActive)
        {
            _viewport = viewport;
            isActive.Subscribe(x => _isActive = x); // todo: remove isActive try implement via layer enable property
            _viewport.ViewportChanged += (_, _) => { if (_isActive) DataHasChanged(); };
        }

        public double KilometerInterval
        {
            get => _kilometerInterval;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(KilometerInterval));
                _kilometerInterval = value;
                if (_isActive) DataHasChanged();
            }
        }

        public string? CRS { get; set; }

        public MRect? GetExtent() => _viewport.Extent;

        public async Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
        {
            var extent = GetExtent();
            if (extent == null || KilometerInterval < 0)
                return await Task.FromResult<IEnumerable<IFeature>>(Enumerable.Empty<IFeature>());
            var meterStep = KilometerInterval * 1000;
            
            var xStart = Math.Ceiling(extent.BottomLeft.X / meterStep) * meterStep;
            var yStart = Math.Ceiling(extent.BottomLeft.Y / meterStep) * meterStep;

            var gridLines = new List<IFeature>();
            await Task.Run(() => 
            {
                for (var i = xStart; i < extent.TopRight.X; i += meterStep)
                {
                    var point1 = new Coordinate(i, extent.TopRight.Y);
                    var point2 = new Coordinate(i, extent.BottomLeft.Y);
                    gridLines.Add(CreateLine(point1, point2));
                }

                for (var i = yStart; i < extent.TopRight.Y; i += meterStep)
                {
                    var point1 = new Coordinate(extent.TopRight.X, i);
                    var point2 = new Coordinate(extent.BottomLeft.X, i);
                    gridLines.Add(CreateLine(point1, point2));
                }
            });

            return gridLines;
        }

        private static GeometryFeature CreateLine(Coordinate p1, Coordinate p2)
            => new GeometryFeature { Geometry = new LineString(new[] { p1, p2 }) };

        public void DataHasChanged() => DataChanged?.Invoke(this, new DataChangedEventArgs());
    }
}