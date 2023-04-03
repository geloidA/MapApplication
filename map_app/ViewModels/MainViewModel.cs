using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Notification;
using map_app.Editing;
using map_app.Models;
using map_app.Network;
using map_app.Services;
using map_app.Services.IO;
using map_app.Services.Layers;
using map_app.ViewModels.Controls;
using Mapsui;
using Mapsui.Nts.Extensions;
using Mapsui.UI.Avalonia;
using Mapsui.UI.Avalonia.Extensions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MColor = Mapsui.Styles.Color;

namespace map_app.ViewModels;

public class MainViewModel : ViewModelBase
{
    private DataState _dataState;
    private bool _isRightWasPressed;
    private const double LeftBorderMap = -20037494;
    private MapState? _currentFileMapState;
    private int _deliveryPort;
    private readonly EditManipulation _editManipulation = new();
    private readonly ObservableAsPropertyHelper<bool> _isBaseGraphicUnderPointer;
    private readonly ObservableAsPropertyHelper<bool> _isOrthodromeUnderPointer;
    private readonly MapStateListener _mapStateListener;
    private readonly LayersManageViewModel layersManageViewModel;
    private readonly SettingsViewModel settingsViewModel;

    internal DataState DataState
    {
        get => _dataState;
        set
        {
            this.RaiseAndSetIfChanged(ref _dataState, value);
            Title = GetTitleText(value);
        }
    }

    internal bool IsBaseGraphicUnderPointer => _isBaseGraphicUnderPointer.Value;
    internal bool IsOrthodromeUnderPointer => _isOrthodromeUnderPointer.Value;

    internal bool IsRulerActivated { get; set; }

    internal MapControl MapControl { get; }

    internal GraphicsLayer GraphicsLayer { get; }

    private readonly EditManager _editManager;

    [Reactive]
    private BaseGraphic? UnderPointerGraphic { get; set; }

    [Reactive]
    internal string? DeliveryIPAddress { get; set; }

    internal EditMode EditMode
    {
        get => _editManager.EditMode;
        set => _editManager.EditMode = value;
    }

    internal MColor EditManagerColor
    {
        get => _editManager.Color;
        set => _editManager.Color = value;
    }

    public int DeliveryPort
    {
        get => _deliveryPort;
        internal set => this.RaiseAndSetIfChanged(ref _deliveryPort, value);
    }

    [Reactive]
    internal string Title { get; set; } = "Подготовка полетного задания";

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
        GraphicsLayer = (GraphicsLayer)MapControl.Map!.Layers.FindLayer(nameof(GraphicsLayer)).Single();
        GraphicsLayer.LayersFeatureChanged += (_, _) => MapControl.RefreshGraphics();
        _deliveryPort = int.Parse(App.Config["default_port"]
            ?? throw new InvalidOperationException("Can't find default port from appsettings.json"));
        _mapStateListener = new(_deliveryPort, this);
        _mapStateListener.RunAsync(stopPredicate: () => true);
        _editManager = new(this, new Mapsui.MRect(LeftBorderMap, LeftBorderMap, -LeftBorderMap, -LeftBorderMap));
        GraphicsPopupViewModel = new(this);
        NavigationPanelViewModel = new(this);
        layersManageViewModel = new(MapControl.Map);
        settingsViewModel = new(this);
        AuxiliaryPanelViewModel = new(this);
        _isBaseGraphicUnderPointer = this
            .WhenAnyValue(x => x.UnderPointerGraphic)
            .Select(f => f is BaseGraphic)
            .ToProperty(this, x => x.IsBaseGraphicUnderPointer);
        _isOrthodromeUnderPointer = this
            .WhenAnyValue(x => x.UnderPointerGraphic)
            .Select(f => f is OrthodromeGraphic)
            .ToProperty(this, x => x.IsOrthodromeUnderPointer);
        GraphicsLayer.LayersFeatureChanged += (sender, _) =>
        {
            var graphics = (GraphicsLayer)sender;
            HaveGraphics = graphics.Features.Any();
            DataState = DataState.Unsaved;
        };
        InitializeCommands();
    }

    private void InitializeCommands()
    {
        OpenLayersManageView = ReactiveCommand.CreateFromTask(async () => await ShowLayersManageDialog.Handle(layersManageViewModel));
        OpenSettingsView = ReactiveCommand.CreateFromTask(async () => await ShowSettingsDialog.Handle(settingsViewModel));
        var graphicUnderPointer = this.WhenAnyValue(x => x.IsBaseGraphicUnderPointer);
        OpenGraphicEditingView = ReactiveCommand.CreateFromTask(async () =>
        {
            var vm = new GraphicAddEditViewModel(UnderPointerGraphic ?? throw new NullReferenceException(), MapControl);
            var result = await ShowGraphicEditingDialog.Handle(vm);
            if (result == DialogResult.OK)
                DataState = DataState.Unsaved;
        },
        canExecute: graphicUnderPointer);
        var haveAnyGraphic = this.WhenAnyValue(x => x.HaveGraphics);
        SaveGraphicStateInFile = ReactiveCommand.CreateFromTask(SaveGraphicStateInFileImpl,
                                                                canExecute: haveAnyGraphic);
        LoadGraphicStateAsync = ReactiveCommand.CreateFromTask(LoadGraphicStateAsyncImpl);
        var canSave = this
            .WhenAnyValue(x => x.DataState)
            .Select(appState => appState == DataState.Unsaved && _currentFileMapState != null);
        SaveGraphicStateInOpenedFile = ReactiveCommand.Create(() =>
        {
            _currentFileMapState!.Graphics = GraphicsLayer.Features;
            SaveGraphics(_currentFileMapState);
            DataState = DataState.Saved;
        },
        canExecute: canSave);
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
        SendViaTCP = ReactiveCommand.CreateFromTask(SendViaTCPImpl, canExecute: haveAnyGraphic);
        OpenExportOrhodromeIntervalsView = ReactiveCommand.CreateFromTask(async () =>
        {
            var vm = new ExportOrhodromeIntervalsViewModel((OrthodromeGraphic)UnderPointerGraphic!);
            await ShowExportOrhodromeIntervalsDialogAsync.Handle(vm);
        });
        CopyGraphic = ReactiveCommand.Create(() => GraphicsLayer.Add(UnderPointerGraphic!.Copy()));
    }

    private string GetTitleText(DataState appState)
    {
        return appState switch
        {
            DataState.None => Title,
            DataState.Saved => Title[0] == '*' ? Title[1..] : Title,
            DataState.Unsaved => Title[0] == '*' ? Title : '*' + Title,
            _ => throw new NotImplementedException()
        };
    }

    private async Task SendViaTCPImpl()
    {
        if (string.IsNullOrEmpty(DeliveryIPAddress))
        {
            OpenSettingsView?.Execute(null);
            return;
        }
        var remotePoint = new IPEndPoint(IPAddress.Parse(DeliveryIPAddress), DeliveryPort);
        _currentFileMapState ??= new MapState { Graphics = GraphicsLayer.Features };
        var (Success, Message) = await TrySendData(remotePoint, _currentFileMapState.ToJsonBytes());
        if (Success)
            ShowNotification(Message, "Информация", Colors.LightBlue);
        else
            ShowNotification(Message, "Ошибка", Colors.Red);
    }

    private static async Task<(bool Success, string Message)> TrySendData(IPEndPoint remotePoint, byte[] data)
    {
        try
        {
            await SendDataAsync(remotePoint, data);
            return (true, "Состояние карты отправлено");
        }
        catch (SocketException e)
        {
            return (false, e.Message);
        }
    }

    private static async Task SendDataAsync(IPEndPoint remotePoint, byte[] data)
    {
        using var tcpClient = new TcpClient();
        tcpClient.Connect(remotePoint);
        using var stream = tcpClient.GetStream();
        await stream.WriteAsync(data);
    }

    internal ICommand? OpenLayersManageView { get; private set; }
    internal ICommand? OpenSettingsView { get; private set; }
    internal ICommand? OpenGraphicEditingView { get; private set; }
    internal ICommand? SaveGraphicStateInOpenedFile { get; private set; }
    internal ICommand? SaveGraphicStateInFile { get; private set; }
    internal ICommand? LoadGraphicStateAsync { get; private set; }
    internal ICommand? ExitApp { get; private set; }
    internal ICommand? ImportImages { get; private set; }
    internal ICommand? SendViaTCP { get; private set; }
    internal ICommand? CopyGraphic { get; private set; }
    internal ICommand? OpenExportOrhodromeIntervalsView { get; private set; }

    internal INotificationMessageManager NotificationManager { get; } = new NotificationMessageManager();

    internal readonly Interaction<LayersManageViewModel, Unit> ShowLayersManageDialog = new();
    internal readonly Interaction<GraphicAddEditViewModel, DialogResult> ShowGraphicEditingDialog = new();
    internal readonly Interaction<MapStateSaveViewModel, MapState?> ShowSaveGraphicStateDialog = new();
    internal readonly Interaction<List<string>, string?> ShowOpenFileDialogAsync = new();
    internal readonly Interaction<Unit, string[]?> ShowImportImagesDialogAsync = new();
    internal readonly Interaction<ExportOrhodromeIntervalsViewModel, Unit> ShowExportOrhodromeIntervalsDialogAsync = new();
    internal readonly Interaction<SettingsViewModel, Unit> ShowSettingsDialog = new();

    internal void AccessOnlyGraphic(object? sender, CancelEventArgs e)
    {
        e.Cancel = !IsBaseGraphicUnderPointer ||
            (_editManager.EditMode != EditMode.None && EditMode.DrawingMode.HasFlag(_editManager.EditMode));
    }

    internal void MapControlOnPointerMoved(object? sender, PointerEventArgs args)
    {
        var point = args.GetCurrentPoint(MapControl);
        var screenPosition = args.GetPosition(MapControl).ToMapsui();

        if (point.Properties.IsLeftButtonPressed)
        {
            _editManipulation.Manipulate(MouseState.Dragging, screenPosition,
                _editManager, MapControl);
        }
        else
        {
            _editManipulation.Manipulate(MouseState.Moving, screenPosition,
                _editManager, MapControl);
        }
    }

    internal void MapControlOnPointerReleased(object? sender, PointerReleasedEventArgs args)
    {
        var screenPosition = args.GetPosition(MapControl).ToMapsui();

        if (_isRightWasPressed) // need for escape drawing by right click
        {
            _isRightWasPressed = false;
            return;
        }

        if (EditMode.OrthodromeEditing.HasFlag(_editManager.EditMode))
            screenPosition = GetOrthodromeNextCoordinate(screenPosition);

        if (MapControl.Map != null)
            MapControl.Map.PanLock = _editManipulation.Manipulate(MouseState.Up,
                screenPosition, _editManager, MapControl);
    }

    internal void MapControlOnPointerPressed(object? sender, PointerPressedEventArgs args)
    {
        var point = args.GetCurrentPoint(MapControl);

        if (MapControl.Map == null)
            return;

        var screenPosition = args.GetPosition(MapControl).ToMapsui();

        if (point.Properties.IsRightButtonPressed)
        {
            _isRightWasPressed = true;
            var infoArgs = MapControl.GetMapInfo(screenPosition);
            UnderPointerGraphic = infoArgs?.Feature as BaseGraphic;
            return;
        }

        if (EditMode.OrthodromeEditing.HasFlag(_editManager.EditMode))
            screenPosition = GetOrthodromeNextCoordinate(screenPosition);

        if (args.ClickCount > 1)
        {
            MapControl.Map.PanLock = _editManipulation.Manipulate(MouseState.DoubleClick,
                screenPosition, _editManager, MapControl);
            args.Handled = true;
        }
        else
        {
            MapControl.Map.PanLock = _editManipulation.Manipulate(MouseState.Down,
                screenPosition, _editManager, MapControl);
        }
    }

    private MPoint GetOrthodromeNextCoordinate(MPoint screenPosition)
    {
        var firstPoint = MapControl
            .GetMapInfo(screenPosition)?
            .MapInfoRecords
            .FirstOrDefault(x => x.Feature is PointGraphic)?
            .Feature;
        return firstPoint != null
            ? MapControl.Viewport.WorldToScreen(((PointGraphic)firstPoint).Coordinates.Single().ToMPoint())
            : screenPosition;
    }

    internal void DeleteGraphic()
    {
        if (UnderPointerGraphic is null)
            throw new NullReferenceException("Graphic was null");
        var success = _editManager.GraphicLayer.TryRemove(UnderPointerGraphic);
        if (success) DataState = DataState.Unsaved;
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
    }

    internal async void UpdateGraphics(IEnumerable<BaseGraphic> newGraphics, bool clearing = true)
    {
        var haveFailed = await LoadPointImagesAsync(newGraphics.Where(x => x is PointGraphic));
        if (haveFailed)
            ShowNotification("Некоторых изображений - нет", "Информация", Colors.LightBlue);
        if (clearing)
            GraphicsLayer.Clear();
        GraphicsLayer.AddRange(newGraphics);
    }

    private static async Task<bool> LoadPointImagesAsync(IEnumerable<BaseGraphic> graphics)
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
        Title = $"{Path.GetFileName(state.FileLocation)} - Подготовка полетного задания";
        DataState = DataState.Saved;
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
        var vm = new MapStateSaveViewModel(GraphicsLayer.Features);
        var mapState = await ShowSaveGraphicStateDialog.Handle(vm);
        if (mapState is null) return;
        UpdateCurrentState(mapState);
    }

    private static async void SaveGraphics(MapState state) => await MapStateJsonMarshaller.SaveAsync(state, state.FileLocation);

    internal void CancelDrawing() => _editManager.CancelDrawing();

    internal void EndIncompleteEditing() => _editManager.EndIncompleteEditing();
}