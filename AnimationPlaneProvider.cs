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
    public class AnimationPlaneProvider : MemoryProvider, IDynamic
    {
        private IEnumerable<PointFeature> _previousFeatures = new List<PointFeature>();
        private AircraftDataSource _aircraftData;

        public AnimationPlaneProvider()
        {
            _aircraftData = new AircraftDataSource();
            _aircraftData.DataChanged += (s, e) => DataHasChanged();
        }

        public event DataChangedEventHandler? DataChanged;

        public override Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
        {
            var features = new List<PointFeature>();

            foreach (var aircraft in _aircraftData.Aircrafts)
            {
                var idAsString = aircraft.ID.ToString(CultureInfo.InvariantCulture);

                features.Add(new PointFeature(new MPoint(aircraft.Point))
                {
                    ["ID"] = aircraft.ID,
                    ["rotation"] = AngleOf(aircraft.Point, FindPreviousPosition(idAsString)) + 180
                });
            }

            _previousFeatures = features;
            return Task.FromResult((IEnumerable<IFeature>)features);
        }

        private MPoint? FindPreviousPosition(string countAsString)
        {
            return _previousFeatures.FirstOrDefault(f => f["ID"]?.ToString() == countAsString)?.Point;
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
    }
}