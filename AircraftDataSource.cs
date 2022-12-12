using System;
using System.Collections.Generic;
using Mapsui.Fetcher;
using map_app.Network;

namespace map_app
{
    public class AircraftDataSource
    {
        private List<AircraftClient> _aircrafts = new List<AircraftClient>();
        public IReadOnlyList<AircraftClient> Aircrafts => _aircrafts;

        public AircraftDataSource() 
        {
            var server = new AircraftServer(1234, this);
            server.RunAsync(() => true, server.ProcessClientAsync);
        }

        public event DataChangedEventHandler? DataChanged;

        public void AddAircraft(AircraftClient aircraft)
        {
            aircraft.DataChanged += (s, e) => DataHasChanged();
            _aircrafts.Add(aircraft);
        }

        public AircraftClient GetAircraftById(int id)
        {
            if (id < 0 || id >= _aircrafts.Count)
            {
                throw new ArgumentException($"wrong id - was {id}");
            }

            return _aircrafts[id];
        }

        public void DataHasChanged()
        {
            OnDataChanged();
        }

        private void ChangePoint()
        {            
            DataHasChanged();
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, new DataChangedEventArgs(null, false, null));
        }
    }
}