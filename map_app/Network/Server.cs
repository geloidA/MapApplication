using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace map_app.Network
{
    public abstract class Server
    {
        private int _port;
        public Server(int port)
        {
            _port = port;
        }
        
        public async void RunAsync(Func<bool> stopPredicate, Func<TcpClient, Task> processClientAsync)
        {
            var tcpListener = TcpListener.Create(_port);
            tcpListener.Start();
            while (stopPredicate())
            {
                var tcpClient = await tcpListener.AcceptTcpClientAsync();
                processClientAsync(tcpClient);
            }
        }
    }
}