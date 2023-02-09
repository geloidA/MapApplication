using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Mapsui.Layers;
using map_app.Models;
using Mapsui.Fetcher;
using System.Buffers;
using System.Text.Json;

namespace map_app.Network
{
    public class AircraftClient : ServerClient, IDynamic
    {
        private Aircraft? aircraft;

        public event DataChangedEventHandler? DataChanged;

        public AircraftClient(TcpClient client) : base(client)
        {
            
        }

        public Aircraft? GetAircraft()
        {
            return aircraft;
        }

        public override async Task ProcessAsync()
        {
            while (true)
            {
                var bytes = await ReadFromStreamAsync(4096);
                var newState = ParseFromBytes(bytes);
                Console.WriteLine($"Client sended json");
                if (newState is null)
                    continue;
                if (aircraft != null && aircraft.Id != newState.Id)
                {
                    throw new Exception($"Id was \"{newState.Id}\" but need \"{aircraft.Id}\"");
                }
                aircraft = newState;
                DataHasChanged();
            }
        }

        private Aircraft? ParseFromBytes((byte[] Buf, int Count) bytes)
        {
            var doc = JsonDocument.Parse(new ReadOnlySequence<byte>(bytes.Buf, 0, bytes.Count));
            return doc.Deserialize<Aircraft>();
        } 

        public void DataHasChanged() => OnDataChanged();

        private void OnDataChanged() => DataChanged?.Invoke(this, new DataChangedEventArgs());
    }
}