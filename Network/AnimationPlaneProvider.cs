using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Mapsui;
using System.Threading.Tasks;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.Utilities;

namespace map_app.Network
{
    public class AnimationAircraftsProvider : MemoryProvider, IDynamic
    {
        private IEnumerable<PointFeature> _previousFeatures = new List<PointFeature>();
        private AircraftDataSource _aircraftDataSource;
        private const double Delta = 1e-5;
        public AnimationAircraftsProvider()
        {
            _aircraftDataSource = new AircraftDataSource();
            _aircraftDataSource.DataChanged += (s, e) => DataHasChanged();
        }

        public event DataChangedEventHandler? DataChanged;

        public override Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
        {
            var features = new List<PointFeature>();

            foreach (var aircraft in _aircraftDataSource.Aircrafts)
            {
                var idAsString = aircraft!.Id.ToString(CultureInfo.InvariantCulture);
                var aircraftCoordinates = SphericalMercator.FromLonLat(lon: aircraft.Longtitude, lat: aircraft.Latitude);
                var previousPoint = FindPreviousPosition(idAsString);
                var aircraftPoint = new MPoint(aircraftCoordinates.x, aircraftCoordinates.y);
                features.Add(new PointFeature(aircraftPoint)
                {
                    ["ID"] = idAsString,
                    ["rotation"] = IsPositionChange(aircraftPoint, previousPoint)
                            ? AngleOf(aircraftPoint, previousPoint) - 90
                            : FindPreviousRotation(idAsString)
                });
            }

            _previousFeatures = features;
            return Task.FromResult((IEnumerable<IFeature>)features);
        }

        private bool IsPositionChange(MPoint point1, MPoint? point2)
        {
            if (point2 == null) return true;
            return Math.Abs(point1.X - point2.X) > Delta
                || Math.Abs(point1.Y - point2.Y) > Delta;
        }

        private double FindPreviousRotation(string idAsString)
        {
            return (double)_previousFeatures.FirstOrDefault(f => f["ID"]?.ToString() == idAsString)?["rotation"]!;
        }

        private MPoint? FindPreviousPosition(string idAsString)
        {
            return _previousFeatures.FirstOrDefault(f => f["ID"]?.ToString() == idAsString)?.Point;
        }

        public static double AngleOf(MPoint point1, MPoint? point2)
        {
            if (point2 == null) return 0;
            double result = Algorithms.RadiansToDegrees(Math.Atan2(point1.Y - point2.Y, point2.X - point1.X));
            return (result < 0) ? (360.0 + result) : result;
        }

        public void DataHasChanged() => OnDataChanged();

        private void OnDataChanged() => DataChanged?.Invoke(this, new DataChangedEventArgs(null, false, null));
    }
}