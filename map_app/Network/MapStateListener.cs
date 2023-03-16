using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;
using map_app.Models;
using map_app.Services;
using map_app.ViewModels;

namespace map_app.Network;

public class MapStateListener
{
    private bool _active;
    private readonly MainViewModel _mainVM;
    private readonly TcpListener _listener;

    public MapStateListener(int port, MainViewModel vm)
    {
        _listener = TcpListener.Create(port);
        _mainVM = vm;
    }
    
    public async void RunAsync(Func<bool> stopPredicate)
    {
        if (_active) throw new InvalidOperationException("The server is already running");
        _listener.Start();
        _active = true;
        try
        {
            while (stopPredicate())
                await Accept(await _listener.AcceptTcpClientAsync());
        }
        finally { _listener.Stop(); }
    }

    private async Task Accept(TcpClient handler)
    {
        var state = await HandleClientAsync(handler);
        handler.Close();
        if (state is not null)
            _mainVM.UpdateGraphics(state.Graphics ?? Enumerable.Empty<BaseGraphic>(), false);
        else _mainVM.ShowNotification("Не удалось загрузить данные по сети", "Информация", Colors.LightBlue);
    }

    private async Task<MapState?> HandleClientAsync(TcpClient handler)
    {
        MapState? state = null;
        using (var stream = handler.GetStream())
        {
            var buffer = new byte[1024];
            var jsonBuilder = new StringBuilder();
            var numberOfBytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            while (handler.Connected && numberOfBytesRead > 0)
            {
                jsonBuilder.Append(Encoding.UTF8.GetString(buffer, 0, numberOfBytesRead));
                numberOfBytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            }
            state = MapStateJsonSerializer.Deserialize(jsonBuilder.ToString());
        }
        return state;
    }
}