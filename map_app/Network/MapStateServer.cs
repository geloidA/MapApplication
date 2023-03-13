using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace map_app.Network;

public class MapStateServer
{
    private int _port;
    private bool _active;
    private TcpListener _listener;

    public MapStateServer(int port)
    {
        _port = port;
        _listener = TcpListener.Create(_port);
    }
    
    public async void RunAsync(Func<bool> stopPredicate)
    {
        if (_active) throw new InvalidOperationException("The server is already running");
        _listener.Start();
        _active = true;
        while (stopPredicate())
        {
            var tcpClient = await _listener.AcceptTcpClientAsync();
            await ProcessClientAsync(tcpClient);
        }
    }

    private async Task ProcessClientAsync(TcpClient client)
    {
        var clientStream = client.GetStream();
        
    }

    public void Stop()
    {
        if (!_active) throw new InvalidOperationException("The server is not running");
        _listener.Stop();
    }
}