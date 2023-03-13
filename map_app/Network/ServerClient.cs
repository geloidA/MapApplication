using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace map_app.Network
{
    public abstract class ServerClient : IDisposable
    {
        private NetworkStream stream;
        
        public ServerClient(TcpClient client)
        {
            stream = client.GetStream();
        }

        public abstract Task ProcessClientAsync();

        public async Task<(byte[] Buf, int Count)> ReadFromStreamAsync(int bufSize)
        {
            var buf = new byte[4096];
            var byteCount = await stream.ReadAsync(buf, 0, buf.Length);
            return (buf, byteCount);
        }

        public void Dispose() => stream.Dispose();
    }
}