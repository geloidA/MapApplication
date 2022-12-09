using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Mapsui;
using System.Threading.Tasks;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Utilities;

namespace map_app
{
    public class AnimationPlaneProvider : MemoryProvider, IDynamic, IDisposable
    {
        private IEnumerable<PointFeature> _previousFeatures = new List<PointFeature>();
        private AircraftDataSource _aircraftData;
        private MPoint? _previousPoint;

        public AnimationPlaneProvider()
        {
            _aircraftData = new AircraftDataSource();
            _previousPoint = new MPoint(_aircraftData.AircraftPoint);
            _aircraftData.DataChanged += (s, e) => DataHasChanged();
        }

        public event DataChangedEventHandler? DataChanged;

        public override Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
        {
            var features = new List<PointFeature>();

            var currentPoint = new MPoint(_aircraftData.AircraftPoint.X, _aircraftData.AircraftPoint.Y);
            features.Add(new PointFeature(currentPoint)
                {
                    ["rotation"] = AngleOf(currentPoint, _previousPoint) + 180
                });
            
            _previousPoint = currentPoint;
            _previousFeatures = features;
            return Task.FromResult((IEnumerable<IFeature>)features);
        }

        public static double AngleOf(MPoint point1, MPoint? point2)
        {
            if (point2 == null) return 0;
            double result = Algorithms.RadiansToDegrees(Math.Atan2(point1.Y - point2.Y, point2.X - point1.X));
            return (result < 0) ? (360.0 + result) : result;
        }

        public void DataHasChanged()
        {
            OnDataChanged();
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, new DataChangedEventArgs(null, false, null));
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _aircraftData.Dispose();
            }
        }
    }
}