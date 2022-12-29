using System;
using System.Collections.Generic;
using Mapsui.Fetcher;
using System.Linq;
using map_app.Network;
using map_app.Models;

namespace map_app.Network
{
    public class AircraftDataSource
    {
        private List<AircraftClient> _clients = new List<AircraftClient>();
        public IEnumerable<Aircraft?> Aircrafts => _clients.Select(c => c.GetAircraft());

        public AircraftDataSource()
        {
            var server = new AircraftServer(1234, this);
            server.RunAsync(() => true, server.ProcessClientAsync);
        }

        public event DataChangedEventHandler? DataChanged;

        public void AddAircraft(AircraftClient aircraft)
        {
            aircraft.DataChanged += (s, e) => DataHasChanged();
            _clients.Add(aircraft);
        }

        public AircraftClient GetAircraftById(int id)
        {
            if (id < 0 || id >= _clients.Count)
            {
                throw new ArgumentException($"wrong id - was {id}");
            }

            return _clients[id];
        }

        public void DataHasChanged() => OnDataChanged();

        private void OnDataChanged() => DataChanged?.Invoke(this, new DataChangedEventArgs(null, false, null));
    }
}