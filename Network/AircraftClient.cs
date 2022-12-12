using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Fetcher;
using map_app.Converters;
using System.Text;

namespace map_app.Network
{
    public class AircraftClient : ServerClient, IDynamic
    {
        public int ID { get; }
        public double Longtitude { get; private set; }
        public double Latitude { get; private set; }

        public event DataChangedEventHandler? DataChanged;

        public AircraftClient(int id, TcpClient client) : base(client)
        {
            ID = id;
        }

        public override async Task ProcessAsync()
        {
            var strCoordinates = string.Empty;
            while (strCoordinates != "quit\n")
            {
                var bytes = await ReadFromStreamAsync(4096);
                strCoordinates = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                Console.WriteLine($"Client input: {strCoordinates}");
                var coordinates = LonLatConverter.ConvertFrom(strCoordinates, " ");
                Longtitude = coordinates.Lon;
                Latitude = coordinates.Lat;
                DataHasChanged();
            }
        }

        public void DataHasChanged()
        {
            OnDataChanged();
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, new DataChangedEventArgs());
        }
    }
}