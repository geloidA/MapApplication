using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Fetcher;
using Mapsui;
using System.Text;

namespace map_app.Network
{
    public class Aircraft : IDisposable, IDynamic
    {
        private NetworkStream stream;

        public int ID { get; }

        public event DataChangedEventHandler? DataChanged;

        public MPoint Point { get; private set; }

        public Aircraft(int id, TcpClient client)
        {
            ID = id;
            Point = new MPoint();
            stream = client.GetStream();
        }

        public async Task ProcessAsync()
        {
            var strPoint = string.Empty;
            while (strPoint != "quit\n")
            {
                strPoint = await ReadFromStreamAsync();
                Console.WriteLine($"{strPoint}");
                Point = ToMPoint(strPoint);
                DataHasChanged();
            }
        }

        public MPoint ToMPoint(string strPoint)
        {
            var coordinates = strPoint.Split()
                    .Take(2)
                    .Select(int.Parse)
                    .ToArray();

            if (coordinates.Length != 2)
            {
                throw new ArgumentException();
            }

            return new MPoint(coordinates[0], coordinates[1]);
        }

        async Task<string> ReadFromStreamAsync()
        {
            var buf = new byte[4096];
            var byteCount = await stream.ReadAsync(buf, 0, buf.Length);
            return Encoding.UTF8.GetString(buf, 0, byteCount);
        }

        public void DataHasChanged()
        {
            OnDataChanged();
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, new DataChangedEventArgs());
        }

        public void Dispose()
        {
            stream.Dispose();
        }
    }
}