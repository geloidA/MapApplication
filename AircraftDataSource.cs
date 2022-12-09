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
using System.Threading;

namespace map_app
{
    public class AircraftDataSource : IDisposable
    {
        NetworkStream s;

        public AircraftDataSource() 
        { 
            RunServer();
        }

#region Network
        async void RunServer()
        {
            var tcpListener = TcpListener.Create(1234);
            tcpListener.Start();
            while (true)
            {
                var tcpClient = await tcpListener.AcceptTcpClientAsync();
                Console.WriteLine("Client connected");
                processClientTearOff(tcpClient);
            }
        }

        async Task processClientTearOff(TcpClient c)
        {
            s = c.GetStream();
            await StartGettingPoints();
        }

        public async Task StartGettingPoints()
        {
            var strPoint = string.Empty;
            while (strPoint != "quit\n")
            {
                strPoint = await ReadFromStreamAsync();
                Console.WriteLine($"{strPoint}");
                AircraftPoint = ToMPoint(strPoint);
                DataHasChanged();
            }
        }

        private MPoint ToMPoint(string strPoint)
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
            var byteCount = await s.ReadAsync(buf, 0, buf.Length);
                
            return Encoding.UTF8.GetString(buf, 0, byteCount);
        }
#endregion 
       
        public MPoint AircraftPoint { get; set; } = new(1, 1);

        public event DataChangedEventHandler? DataChanged;

        public void DataHasChanged()
        {
            OnDataChanged();
        }

        public void Dispose()
        {
            s.Dispose();
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