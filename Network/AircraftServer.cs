using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace map_app.Network
{
    public class AircraftServer : Server
    {
        private int aircraftCount;
        private AircraftDataSource _aircraftSource;

        public AircraftServer(int port, AircraftDataSource aircraftSource) : base(port)
        {
            _aircraftSource = aircraftSource;
        }

        public async Task ProcessClientAsync(TcpClient c)
        {
            using (var aircraft = new AircraftClient(aircraftCount, c))
            {
                _aircraftSource.AddAircraft(aircraft);
                aircraftCount++;
                await aircraft.ProcessAsync();
            }
        }
    }
}