using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Mapsui.Fetcher;
using System.Text;
using Mapsui;
using System.Threading.Tasks;
using System.Net.Sockets;
using map_app.Network;
using System.Threading;

namespace map_app
{
    public class AircraftDataSource
    {
        private List<Aircraft> _aircrafts = new List<Aircraft>();

        public IReadOnlyList<Aircraft> Aircrafts => _aircrafts;

        public AircraftDataSource() 
        { 
            var server = new AircraftServer(this);
            server.Run();
        }

        public event DataChangedEventHandler? DataChanged;

        public void AddAircraft(Aircraft aircraft)
        {
            aircraft.DataChanged += (s, e) => DataHasChanged();
            _aircrafts.Add(aircraft);
        }

        public Aircraft GetAircraftById(int id)
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