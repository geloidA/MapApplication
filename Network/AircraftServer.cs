using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace map_app.Network
{
    public class AircraftServer
    {
        private int aircraftCount;
        private AircraftDataSource _aircraftSource;

        public AircraftServer(AircraftDataSource aircraftSource)
        {
            _aircraftSource = aircraftSource;
        }

        public async void Run()
        {
            var tcpListener = TcpListener.Create(1234);
            tcpListener.Start();
            while (true) // тут какое-то разумное условие выхода
            {
                var tcpClient = await tcpListener.AcceptTcpClientAsync();
                processAircraft(tcpClient); // await не нужен
                aircraftCount++;
            }
        }

        private async Task processAircraft(TcpClient c)
        {
            using (var aircraft = new Aircraft(aircraftCount, c))
            {
                _aircraftSource.AddAircraft(aircraft);
                await aircraft.ProcessAsync();
            }
        }
    }
}