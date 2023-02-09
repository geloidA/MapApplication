using System.Threading.Tasks;
using System.Net.Sockets;

namespace map_app.Network
{
    public class AircraftServer : Server
    {
        private AircraftDataSource _aircraftSource;

        public AircraftServer(int port, AircraftDataSource aircraftSource) : base(port)
        {
            _aircraftSource = aircraftSource;
        }

        public async Task ProcessClientAsync(TcpClient c)
        {
            using (var aircraft = new AircraftClient(c))
            {
                _aircraftSource.AddAircraft(aircraft);
                await aircraft.ProcessAsync();
            }
        }
    }
}