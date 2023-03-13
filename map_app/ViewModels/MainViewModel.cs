# region usings
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
#endregion

namespace map_app.ViewModels;

public class MainViewModel : ViewModelBase
{
    #region Private members
    private bool _isRightWasPressed;
    private readonly List<string> _unregisteredImages = new();
    private const double LeftBorderMap = -20037494;
    private MapState? _currentFileMapState;
    private readonly EditManipulation _editManipulation = new();
    private readonly ObservableAsPropertyHelper<bool> _isBaseGraphicUnderPointer;
    private readonly MapStateServer _mapStateServer;
    #endregion

    public bool IsBaseGraphicUnderPointer => _isBaseGraphicUnderPointer.Value;

    internal bool IsRulerActivated { get; set; }

    internal MapControl MapControl { get; }

    internal GraphicsLayer Graphics { get; }
    
    internal EditManager EditManager { get; }

    [Reactive]
    private string? LastFilePath { get; set; }

    [Reactive]
    private BaseGraphic? FeatureUnderPointer { get; set; }

    [Reactive]
    public string? LoadedFileName { get; set; }

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
        _mapStateServer = new MapStateServer(int.Parse(App.Config["default_port"] 
            ?? throw new NullReferenceException()));
        _mapStateServer.RunAsync(() => true);
        MapControl = mapControl;
        MapControl.Map = MapCreator.Create();
        Graphics = (GraphicsLayer)MapControl.Map!.Layers.FindLayer(nameof(GraphicsLayer)).Single();
        EditManager = new EditManager(this);
        EditManager.Extent = new Mapsui.MRect(LeftBorderMap, LeftBorderMap, -LeftBorderMap, -LeftBorderMap);
        GraphicsPopupViewModel = new GraphicsPopupViewModel(this);
        NavigationPanelViewModel = new NavigationPanelViewModel(this);
        AuxiliaryPanelViewModel = new AuxiliaryPanelViewModel(this);        
        _isBaseGraphicUnderPointer = this
            .WhenAnyValue(x => x.FeatureUnderPointer)
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
        var canExecute = this.WhenAnyValue(x => x.IsBaseGraphicUnderPointer);
        OpenGraphicEditingView = ReactiveCommand.CreateFromTask(async () =>
        {
            var vm = new GraphicAddEditViewModel(FeatureUnderPointer ?? throw new NullReferenceException());
            await ShowGraphicEditingDialog.Handle(vm);
        }, canExecute);
        var canSave = this.WhenAnyValue(x => x.HaveGraphics);
        SaveGraphicStateInFile = ReactiveCommand.CreateFromTask(SaveGraphicStateInFileImpl, canSave);
        LoadGraphicStateAsync = ReactiveCommand.CreateFromTask(LoadGraphicStateAsyncImpl);
        var canSaveOpened = this
            .WhenAnyValue(x => x.LastFilePath)
            .Select(file => !string.IsNullOrEmpty(file));
        SaveGraphicStateInOpenedFile = ReactiveCommand.CreateFromTask(async () => 
        {
            _currentFileMapState!.Graphics = Graphics.Features;
            await SaveGraphics(_currentFileMapState);
        }, canSaveOpened);
        ImportImages = ReactiveCommand.CreateFromTask(async () => 
        { 
            var paths = await ShowImportImagesDialogAsync.Handle(Unit.Default);
            if (paths is null) return;
            foreach (var path in paths)
                ImageRegister.EmbedImage(path);
        });
    }

    public ICommand? OpenLayersManageView { get; private set; }

    public ICommand? OpenGraphicEditingView { get; private set; }

    public ICommand? SaveGraphicStateInOpenedFile { get; private set; }

    public ICommand? SaveGraphicStateInFile { get; private set; }

    public ICommand? LoadGraphicStateAsync { get; private set; }

    public ICommand? ImportImages { get; private set; }

    public INotificationMessageManager Manager { get; } = new NotificationMessageManager();

    public Interaction<LayersManageViewModel, MainViewModel> ShowLayersManageDialog { get; } = new();

    public Interaction<GraphicAddEditViewModel, MainViewModel> ShowGraphicEditingDialog { get; } = new();

    public Interaction<MapStateSaveViewModel, MapState?> ShowSaveGraphicStateDialog { get; } = new();
    
    public Interaction<List<string>, string?> ShowOpenFileDialogAsync { get; } = new();

    public Interaction<Unit, string[]?> ShowImportImagesDialogAsync { get; } = new();

    internal void AccessOnlyGraphic(object? sender, CancelEventArgs e) => e.Cancel = !IsBaseGraphicUnderPointer;

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
            FeatureUnderPointer = infoArgs?.Feature as BaseGraphic;
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
        if (FeatureUnderPointer is null)
            throw new NullReferenceException("Graphic was null");
        EditManager.Layer.TryRemove(FeatureUnderPointer);
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
        await LoadPointImagesAsync(state.Graphics?.Where(x => x is PointGraphic) 
            ?? Enumerable.Empty<BaseGraphic>());
        if (_unregisteredImages.Any())
        {
            ShowImageNotification("Проблема загрузки изображений", "Информация", Colors.LightBlue);
            _unregisteredImages.Clear();
        }
        state.FileLocation = loadLocation;
        LoadGraphicsInLayer(state.Graphics, loadLocation);
        _currentFileMapState = state;
    }

    private async Task LoadPointImagesAsync(IEnumerable<BaseGraphic> graphics)
    {
        foreach (var point in graphics.Cast<PointGraphic>())
        {
            if (point.Image != null)
            {
                var bitmapId = await ImageRegister.RegisterAsync(point.Image);
                if (bitmapId is null)
                {
                    _unregisteredImages.Add(point.Image);
                    continue;
                }
                point.GraphicStyle = new Mapsui.Styles.SymbolStyle 
                { 
                    BitmapId = bitmapId.Value,
                    SymbolScale = point.Scale
                };
            }
        }
    }

    private void ShowNotification(string message, string badge, Color color)
    {
        var accentBrush = new SolidColorBrush(color);
        this.Manager
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

    private void ShowImageNotification(string message, string badge, Color color)
    {
        var accentBrush = new SolidColorBrush(color);
        this.Manager
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

    private void LoadGraphicsInLayer(IEnumerable<BaseGraphic>? newGraphics, string loadLocation)
    {
        Graphics.Clear();
        if (newGraphics != null) Graphics.AddRange(newGraphics);
        LastFilePath = loadLocation;
        LoadedFileName = Path.GetFileName(loadLocation);
        MapControl.Refresh();
    }

    private async Task SaveGraphicStateInFileImpl()
    {
        var vm = new MapStateSaveViewModel(Graphics.Features);
        var mapState = await ShowSaveGraphicStateDialog.Handle(vm);
        if (mapState is null) return;
        _currentFileMapState = mapState;
        LastFilePath = mapState.FileLocation;
        LoadedFileName = Path.GetFileName(mapState.FileLocation);
    }

    private async Task SaveGraphics(MapState state)
    {
        await MapStateJsonMarshaller.SaveAsync(state, state.FileLocation);
    }

    public void StopMapStateListener() => _mapStateServer.Stop();
}