using System;
using Mapsui.UI.Avalonia;
using map_app.Services;
using map_app.Editing;
using System.Reactive.Linq;
using System.Linq;
using System.Windows.Input;
using Avalonia.Input;
using Mapsui.UI.Avalonia.Extensions;
using ReactiveUI;
using map_app.Models;
using ReactiveUI.Fody.Helpers;
using System.ComponentModel;
using map_app.ViewModels.Controls;
using map_app.Services.IO;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Avalonia.Notification;
using Avalonia.Media;
using System.Reactive;
using map_app.Network;
using Avalonia.Controls.ApplicationLifetimes;
using System.Net.Sockets;
using System.Net;

namespace map_app.ViewModels;

public class MainViewModel : ViewModelBase
{
    #region private members
    private bool _isRightWasPressed;
    private const double LeftBorderMap = -20037494;
    private MapState? _currentFileMapState;
    private int _deliveryPort;
    private readonly EditManipulation _editManipulation = new();
    private readonly ObservableAsPropertyHelper<bool> _isBaseGraphicUnderPointer;
    private readonly MapStateServer _mapStateServer;
    #endregion

    internal bool IsBaseGraphicUnderPointer => _isBaseGraphicUnderPointer.Value;

    internal bool IsRulerActivated { get; set; }

    internal MapControl MapControl { get; }

    internal GraphicsLayer Graphics { get; }

    internal EditManager EditManager { get; }

    [Reactive]
    private BaseGraphic? UnderPointerFeature { get; set; }

    [Reactive]
    internal string? DeliveryIPAddress { get; set; }

    public int DeliveryPort 
    { 
        get => _deliveryPort;
        internal set => this.RaiseAndSetIfChanged(ref _deliveryPort, value); 
    }

    [Reactive]
    internal string? LoadedFileName { get; set; }

    [Reactive]
    private bool HaveGraphics { get; set; }

    [Reactive]
    internal GraphicsPopupViewModel GraphicsPopupViewModel { get; set; }

    [Reactive]
    internal NavigationPanelViewModel NavigationPanelViewModel { get; set; }

    [Reactive]
    internal AuxiliaryPanelViewModel AuxiliaryPanelViewModel { get; set; }

    public MainViewModel(MapControl mapControl)
    {
        MapControl = mapControl;
        MapControl.Map = MapCreator.Create();
        Graphics = (GraphicsLayer)MapControl.Map!.Layers.FindLayer(nameof(GraphicsLayer)).Single();
        _deliveryPort = int.Parse(App.Config["default_port"] 
            ?? throw new InvalidOperationException("Can't find default port from appsettings.json"));
        _mapStateServer = new MapStateServer(_deliveryPort, this);
        _mapStateServer.RunAsync(() => true);
        EditManager = new EditManager(this);
        EditManager.Extent = new Mapsui.MRect(LeftBorderMap, LeftBorderMap, -LeftBorderMap, -LeftBorderMap);
        GraphicsPopupViewModel = new GraphicsPopupViewModel(this);
        NavigationPanelViewModel = new NavigationPanelViewModel(this);
        AuxiliaryPanelViewModel = new AuxiliaryPanelViewModel(this);
        _isBaseGraphicUnderPointer = this
            .WhenAnyValue(x => x.UnderPointerFeature)
            .Select(f => f as BaseGraphic != null)
            .ToProperty(this, x => x.IsBaseGraphicUnderPointer);
        Graphics.LayersFeatureChanged += (_, _) => HaveGraphics = Graphics.Features.Any();
        InitializeCommands();
    }

    private void InitializeCommands()
    {
        OpenLayersManageView = ReactiveCommand.CreateFromTask(async () =>
        {
            var manager = new LayersManageViewModel(MapControl!.Map!);
            await ShowLayersManageDialog.Handle(manager);
        });
        OpenSettingsView = ReactiveCommand.CreateFromTask(async () =>
        {
            var vm = new SettingsViewModel(this);
            await ShowSettingsDialog.Handle(vm);
        });
        var graphicUnderPointer = this.WhenAnyValue(x => x.IsBaseGraphicUnderPointer);
        OpenGraphicEditingView = ReactiveCommand.CreateFromTask(async () =>
        {
            var vm = new GraphicAddEditViewModel(UnderPointerFeature ?? throw new NullReferenceException());
            await ShowGraphicEditingDialog.Handle(vm);
        }, 
        canExecute: graphicUnderPointer);
        var haveAnyGraphic = this.WhenAnyValue(x => x.HaveGraphics);
        SaveGraphicStateInFile = ReactiveCommand.CreateFromTask(SaveGraphicStateInFileImpl,
                                                                canExecute: haveAnyGraphic);
        LoadGraphicStateAsync = ReactiveCommand.CreateFromTask(LoadGraphicStateAsyncImpl);
        var canSaveOpened = this
            .WhenAnyValue(x => x.LoadedFileName)
            .Select(file => !string.IsNullOrEmpty(file));
        SaveGraphicStateInOpenedFile = ReactiveCommand.CreateFromTask(async () =>
        {
            _currentFileMapState!.Graphics = Graphics.Features;
            await SaveGraphics(_currentFileMapState);
        }, 
        canExecute: canSaveOpened);
        ImportImages = ReactiveCommand.CreateFromTask(async () =>
        {
            var paths = await ShowImportImagesDialogAsync.Handle(Unit.Default);
            if (paths is null) return;
            foreach (var path in paths)
                ImageRegister.EmbedImage(path);
        });
        ExitApp = ReactiveCommand.Create(() =>
        {
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.MainWindow.Close();
        });
        SendViaLAN = ReactiveCommand.Create(SendViaLANImpl,
                                            canExecute: haveAnyGraphic);
    }

    private void SendViaLANImpl()
    {
        if (string.IsNullOrEmpty(DeliveryIPAddress))
        {
            OpenSettingsView?.Execute(null);
            return;
        }
        var endPoint = new IPEndPoint(IPAddress.Parse(DeliveryIPAddress), DeliveryPort);
        if (_currentFileMapState is null)
            _currentFileMapState = new MapState { Graphics = Graphics.Features };
        if (!TrySendData(endPoint, _currentFileMapState.ToJsonBytes(), out string sendMessage))
            ShowNotification(sendMessage, "Ошибка", Colors.Red);
        else 
            ShowNotification(sendMessage, "Информация", Colors.LightBlue);
    }

    private bool TrySendData(IPEndPoint endPoint, byte[] data, out string sendMessage)
    {
        sendMessage = "Состояние карты отправлено";
        try
        {
            SendData(endPoint, data);
            return true;
        }
        catch (SocketException e)
        {
            sendMessage = e.Message;
            return false;
        }
    }

    private void SendData(IPEndPoint endPoint, byte[] data)
    {
        using var tcpClient = new TcpClient(endPoint);
        using (var stream = tcpClient.GetStream())
        {
            stream.Write(data, 0, data.Length);
        }
        tcpClient.Close();
    }

    internal ICommand? OpenLayersManageView { get; private set; }
    internal ICommand? OpenSettingsView { get; private set; }
    internal ICommand? OpenGraphicEditingView { get; private set; }
    internal ICommand? SaveGraphicStateInOpenedFile { get; private set; }
    internal ICommand? SaveGraphicStateInFile { get; private set; }
    internal ICommand? LoadGraphicStateAsync { get; private set; }
    internal ICommand? ExitApp { get; private set; }
    internal ICommand? ImportImages { get; private set; }
    internal ICommand? SendViaLAN { get; private set; }

    internal INotificationMessageManager NotificationManager { get; } = new NotificationMessageManager();

    internal readonly Interaction<LayersManageViewModel, Unit> ShowLayersManageDialog = new();
    internal readonly Interaction<GraphicAddEditViewModel, Unit> ShowGraphicEditingDialog = new();
    internal readonly Interaction<MapStateSaveViewModel, MapState?> ShowSaveGraphicStateDialog = new();
    internal readonly Interaction<List<string>, string?> ShowOpenFileDialogAsync = new();
    internal readonly Interaction<Unit, string[]?> ShowImportImagesDialogAsync = new();
    internal readonly Interaction<SettingsViewModel, Unit> ShowSettingsDialog = new();

    internal void AccessOnlyGraphic(object? sender, CancelEventArgs e)
    {
        e.Cancel = !IsBaseGraphicUnderPointer || EditMode.DrawingMode.HasFlag(EditManager.EditMode);
    }

    internal void MapControlOnPointerMoved(object? sender, PointerEventArgs args)
    {
        var point = args.GetCurrentPoint(MapControl);
        var screenPosition = args.GetPosition(MapControl).ToMapsui();
        var worldPosition = MapControl.Viewport.ScreenToWorld(screenPosition);


        if (point.Properties.IsLeftButtonPressed)
        {
            _editManipulation.Manipulate(MouseState.Dragging, screenPosition,
                EditManager, MapControl);
        }
        else
        {
            _editManipulation.Manipulate(MouseState.Moving, screenPosition,
                EditManager, MapControl);
        }
    }

    internal void MapControlOnPointerReleased(object? sender, PointerReleasedEventArgs args)
    {
        var point = args.GetCurrentPoint(MapControl);

        if (_isRightWasPressed) // need for escape drawing by right click
        {
            _isRightWasPressed = false;
            return;
        }

        if (MapControl.Map != null)
            MapControl.Map.PanLock = _editManipulation.Manipulate(MouseState.Up,
                args.GetPosition(MapControl).ToMapsui(), EditManager, MapControl);
    }

    internal void MapControlOnPointerPressed(object? sender, PointerPressedEventArgs args)
    {
        var point = args.GetCurrentPoint(MapControl);

        if (MapControl.Map == null)
            return;

        if (point.Properties.IsRightButtonPressed)
        {
            _isRightWasPressed = true;
            var infoArgs = MapControl.GetMapInfo(args.GetPosition(MapControl).ToMapsui());
            UnderPointerFeature = infoArgs?.Feature as BaseGraphic;
            return;
        }

        if (args.ClickCount > 1)
        {
            MapControl.Map.PanLock = _editManipulation.Manipulate(MouseState.DoubleClick,
                args.GetPosition(MapControl).ToMapsui(), EditManager, MapControl);
            args.Handled = true;
        }
        else
        {
            MapControl.Map.PanLock = _editManipulation.Manipulate(MouseState.Down,
                args.GetPosition(MapControl).ToMapsui(), EditManager, MapControl);
        }
    }

    private void DeleteGraphic()
    {
        if (UnderPointerFeature is null)
            throw new NullReferenceException("Graphic was null");
        EditManager.GraphicLayer.TryRemove(UnderPointerFeature);
    }

    private async Task LoadGraphicStateAsyncImpl()
    {
        var loadLocation = await ShowOpenFileDialogAsync.Handle(new List<string> { "json" });
        if (loadLocation is null)
            return;
        var state = await MapStateJsonMarshaller.LoadAsync(loadLocation);
        if (state is null || !state.IsInitialized)
        {
            ShowNotification(
                "Выбранный файл не удалось преобразовать в объекты",
                "Ошибка",
                Colors.Red);
            return;
        }
        UpdateGraphics(state.Graphics ?? Enumerable.Empty<BaseGraphic>());
        state.FileLocation = loadLocation;
        UpdateCurrentState(state);
        MapControl.RefreshGraphics();
    }

    internal async void UpdateGraphics(IEnumerable<BaseGraphic> newGraphics, bool clearing = true)
    {
        var haveFailed = await LoadPointImagesAsync(newGraphics.Where(x => x is PointGraphic));
        if (haveFailed)
            ShowNotification("Некоторых изображений - нет", "Информация", Colors.LightBlue);
        if (clearing)
            Graphics.Clear();
        Graphics.AddRange(newGraphics);
    }

    private async Task<bool> LoadPointImagesAsync(IEnumerable<BaseGraphic> graphics)
    {
        var haveFailedImagesPaths = false;
        foreach (var point in graphics.Cast<PointGraphic>().Where(x => x.Image != null))
        {
            var bitmapId = await ImageRegister.RegisterAsync(point.Image!);
            if (bitmapId is null)
            {
                haveFailedImagesPaths = true;
                continue;
            }
            point.GraphicStyle = new Mapsui.Styles.SymbolStyle
            {
                BitmapId = bitmapId.Value,
                SymbolScale = point.Scale
            };
        }
        return haveFailedImagesPaths;
    }

    private void UpdateCurrentState(MapState state)
    {
        _currentFileMapState = state;
        LoadedFileName = Path.GetFileName(state.FileLocation);
    }

    internal void ShowNotification(string message, string badge, Color color)
    {
        var accentBrush = new SolidColorBrush(color);
        NotificationManager
            .CreateMessage()
            .Accent(accentBrush)
            .Animates(true)
            .Background("#333")
            .HasBadge(badge)
            .HasMessage(message)
            .Dismiss()
            .WithButton("OK", _ => { })
            .Queue();
    }

    private async Task SaveGraphicStateInFileImpl()
    {
        var vm = new MapStateSaveViewModel(Graphics.Features);
        var mapState = await ShowSaveGraphicStateDialog.Handle(vm);
        if (mapState is null) return;
        UpdateCurrentState(mapState);
    }

    private async Task SaveGraphics(MapState state) => await MapStateJsonMarshaller.SaveAsync(state, state.FileLocation);
}