using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Fetcher;
using System.Text;

namespace map_app.Network
{
    public class Aircraft : IDisposable, IDynamic
    {
        private NetworkStream stream;

        public event DataChangedEventHandler? DataChanged;

        public double Longtitude { get; private set; }
        public double Latitude { get; private set; }
        public double Height { get; private set; }

        public Aircraft(TcpClient client)
        {
            stream = client.GetStream();
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