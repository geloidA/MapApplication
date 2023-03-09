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

namespace map_app.ViewModels;

public class MainViewModel : ViewModelBase
{
    #region Private members
    private bool _isRightWasPressed;
    private readonly List<string> _unregisteredImages = new();
    private const double LeftBorderMap = -20037494;
    private readonly EditManipulation _editManipulation = new();
    private readonly ObservableAsPropertyHelper<bool> _isBaseGraphicUnderPointer;
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
        MapControl = mapControl;
        MapControl.Map = MapCreator.Create();
        Graphics = (GraphicsLayer)MapControl.Map!.Layers.FindLayer(nameof(GraphicsLayer)).Single();
        EditManager = new EditManager(this);
        EditManager.Extent = new Mapsui.MRect(LeftBorderMap, LeftBorderMap, -LeftBorderMap, -LeftBorderMap);
        GraphicsPopupViewModel = new GraphicsPopupViewModel(this);
        NavigationPanelViewModel = new NavigationPanelViewModel(this);
        AuxiliaryPanelViewModel = new AuxiliaryPanelViewModel(this);
        ShowLayersManageDialog = new Interaction<LayersManageViewModel, MainViewModel>();
        OpenLayersManageView = ReactiveCommand.CreateFromTask(async () =>
        {
            var manager = new LayersManageViewModel(MapControl.Map);
            await ShowLayersManageDialog.Handle(manager);
        });

        _isBaseGraphicUnderPointer = this
            .WhenAnyValue(x => x.FeatureUnderPointer)
            .Select(f => f as BaseGraphic != null)
            .ToProperty(this, x => x.IsBaseGraphicUnderPointer);

        var canExecute = this.WhenAnyValue(x => x.IsBaseGraphicUnderPointer);
        ShowGraphicEditingDialog = new Interaction<GraphicAddEditViewModel, MainViewModel>();
        OpenGraphicEditingView = ReactiveCommand.CreateFromTask(async () =>
        {
            var vm = new GraphicAddEditViewModel(FeatureUnderPointer ?? throw new NullReferenceException());
            await ShowGraphicEditingDialog.Handle(vm);
        }, canExecute);
        ShowSaveGraphicStateDialog = new Interaction<MapStateSaveViewModel, string?>();
        Graphics.LayersFeatureChanged += (_, _) => HaveGraphics = Graphics.Features.Any();
        var canSave = this.WhenAnyValue(x => x.HaveGraphics);
        SaveGraphicStateInFile = ReactiveCommand.CreateFromTask(SaveGraphicStateInFileImpl, canSave);
        ShowOpenFileDialogAsync = new Interaction<List<string>, string?>();
        LoadGraphicStateAsync = ReactiveCommand.CreateFromTask(LoadGraphicStateAsyncImpl);
        var canSaveOpened = this
            .WhenAnyValue(x => x.LastFilePath)
            .Select(file => !string.IsNullOrEmpty(file));
        SaveGraphicStateInOpenedFile = ReactiveCommand.CreateFromTask(async () => await SaveGraphics(LastFilePath!, GetCurrentState()), canSaveOpened);
    }

    public ICommand OpenLayersManageView { get; }

    public ICommand OpenGraphicEditingView { get; }

    public ICommand SaveGraphicStateInOpenedFile { get; }

    public ICommand SaveGraphicStateInFile { get; }

    public ICommand LoadGraphicStateAsync { get; }

    public INotificationMessageManager Manager { get; } = new NotificationMessageManager();

    public Interaction<LayersManageViewModel, MainViewModel> ShowLayersManageDialog { get; }

    public Interaction<GraphicAddEditViewModel, MainViewModel> ShowGraphicEditingDialog { get; }

    public Interaction<MapStateSaveViewModel, string?> ShowSaveGraphicStateDialog { get; }
    
    public Interaction<List<string>, string?> ShowOpenFileDialogAsync { get; }

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
        await LoadPointImagesAsync(state.Graphics);
        if (_unregisteredImages.Any())
        {
            ShowImageNotification("Проблема загрузки изображений", "Информация", Colors.LightBlue);
            _unregisteredImages.Clear();
        }
        LoadGraphicsInLayer(state.Graphics, loadLocation);
    }

    private async Task LoadPointImagesAsync(IEnumerable<BaseGraphic> graphics)
    {
        foreach (var graphic in graphics.Where(x => x is PointGraphic))
        {
            var point = (PointGraphic)graphic;
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
                    SymbolScale = 0.1 
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

    private void LoadGraphicsInLayer(IEnumerable<BaseGraphic> newGraphics, string loadLocation)
    {
        Graphics.Clear();
        Graphics.AddRange(newGraphics);
        LastFilePath = loadLocation;
        LoadedFileName = Path.GetFileName(loadLocation);
        MapControl.Refresh();
    }

    private async Task SaveGraphicStateInFileImpl()
    {
        var vm = new MapStateSaveViewModel(Graphics.Features);
        var saveLocation = await ShowSaveGraphicStateDialog.Handle(vm);
        if (saveLocation is null) return;
        LastFilePath = saveLocation;
        LoadedFileName = Path.GetFileName(saveLocation);
    }

    private async Task SaveGraphics(string location, MapState state)
    {
        await MapStateJsonMarshaller.SaveAsync(state, location);
    }

    private MapState GetCurrentState() => new MapState
    {
        Graphics = Graphics.Features.ToList()
    };
}