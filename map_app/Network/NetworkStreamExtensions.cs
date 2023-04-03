using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace map_app.Network;

public static class NetworkStreamExtensions
{
    public static async Task<int> ReadAsync(this NetworkStream stream, byte[] buffer, int offset, int count, int timeOutMillis)
    {
        var ReciveCount = 0;
        var receiveTask = Task.Run(async () => { ReciveCount = await stream.ReadAsync(buffer.AsMemory(offset, count)); });
        var isReceived = await Task.WhenAny(receiveTask, Task.Delay(timeOutMillis)) == receiveTask;
        if (!isReceived) return -1;
        return ReciveCount;
    }
}