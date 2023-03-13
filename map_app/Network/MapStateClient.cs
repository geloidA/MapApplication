using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace map_app.Network;

public class MapStateClient : ServerClient
{
    public MapStateClient(TcpClient client) : base(client)
    {
        
    }

    public override Task ProcessClientAsync()
    {
        throw new NotImplementedException();
    }
}