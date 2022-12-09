using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Mapsui.Fetcher;
using Mapsui;
using System.Threading;

namespace map_app
{
    public class AircraftDataSource : IDisposable
    {
        private readonly Timer _timer;

        public AircraftDataSource()
        {
            _timer = new Timer(_ => ChangePoint(), null, 0, 100);
        }

        public MPoint AircraftPoint { get; set; } = new(1, 1);

        public event DataChangedEventHandler? DataChanged;

        public void DataHasChanged()
        {
            OnDataChanged();
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        private void ChangePoint()
        {
            AircraftPoint.X = AircraftPoint.X + 1000;
            AircraftPoint.Y = AircraftPoint.Y + 1000;
            DataHasChanged();
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, new DataChangedEventArgs(null, false, null));
        }
    }
}