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
using System.Reactive;
using map_app.Services.IO;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Avalonia.Notification;
using Avalonia.Media;
using Mapsui;

namespace map_app.ViewModels;

public class MainViewModel : ViewModelBase
{
    #region Private members
    private bool _isRightWasPressed;
    private readonly List<string> _unregisteredImages = new();
    private readonly OwnWritableLayer? _savedGraphicLayer;
    private const double LeftBorderMap = -20037494;
    private readonly MapControl _mapControl;
    private readonly EditManager _editManager;
    private readonly EditManipulation _editManipulation = new();
    private readonly ObservableAsPropertyHelper<bool> _isBaseGraphicUnderPointer;
    #endregion

    public bool IsBaseGraphicUnderPointer => _isBaseGraphicUnderPointer.Value;

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
        _mapControl = mapControl;
        _mapControl.Map = MapCreator.Create();
        (_mapControl.Viewport as IViewport)?.SetResolution(1000);
        _savedGraphicLayer = (OwnWritableLayer)_mapControl.Map!.Layers.First(l => l.Name == "Graphic Layer");
        _savedGraphicLayer.Clear();            
        _editManager = new EditManager(_savedGraphicLayer);
        _editManager.Extent = new Mapsui.MRect(LeftBorderMap, LeftBorderMap, -LeftBorderMap, -LeftBorderMap);
        GraphicsPopupViewModel = new GraphicsPopupViewModel(_savedGraphicLayer!, _mapControl);
        NavigationPanelViewModel = new NavigationPanelViewModel(mapControl, _editManager, _savedGraphicLayer!);
        AuxiliaryPanelViewModel = new AuxiliaryPanelViewModel(mapControl);
        ShowLayersManageDialog = new Interaction<LayersManageViewModel, MainViewModel>();
        OpenLayersManageView = ReactiveCommand.CreateFromTask(async () =>
        {
            var manager = new LayersManageViewModel(_mapControl.Map);
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
        ShowSaveGraphicStateDialog = new Interaction<Unit, string?>();
        _savedGraphicLayer.LayersFeatureChanged += (_, _) => HaveGraphics = _savedGraphicLayer.Any();
        var canSave = this.WhenAnyValue(x => x.HaveGraphics);
        SaveGraphicStateInFile = ReactiveCommand.CreateFromTask(SaveGraphicStateInFileImpl, canSave);
        ShowOpenFileDialogAsync = new Interaction<List<string>, string?>();
        LoadGraphicStateAsync = ReactiveCommand.CreateFromTask(LoadGraphicStateAsyncImpl);
        var canSaveOpened = this
            .WhenAnyValue(x => x.LastFilePath)
            .Select(file => !string.IsNullOrEmpty(file));
        SaveGraphicStateInOpenedFile = ReactiveCommand.CreateFromTask(async () => await SaveGraphic(LastFilePath!), canSaveOpened);
    }

    public ICommand OpenLayersManageView { get; }

    public ICommand OpenGraphicEditingView { get; }

    public ICommand SaveGraphicStateInOpenedFile { get; }

    public ICommand SaveGraphicStateInFile { get; }

    public ICommand LoadGraphicStateAsync { get; }

    public INotificationMessageManager Manager { get; } = new NotificationMessageManager();

    public Interaction<LayersManageViewModel, MainViewModel> ShowLayersManageDialog { get; }

    public Interaction<GraphicAddEditViewModel, MainViewModel> ShowGraphicEditingDialog { get; }

    public Interaction<Unit, string?> ShowSaveGraphicStateDialog { get; }
    
    public Interaction<List<string>, string?> ShowOpenFileDialogAsync { get; }

    internal void AccessOnlyGraphic(object? sender, CancelEventArgs e) => e.Cancel = !NavigationPanelViewModel.IsEditMode || !IsBaseGraphicUnderPointer;

    internal void MapControlOnPointerMoved(object? sender, PointerEventArgs args)
    {
        var point = args.GetCurrentPoint(_mapControl);
        var screenPosition = args.GetPosition(_mapControl).ToMapsui();
        var worldPosition = _mapControl.Viewport.ScreenToWorld(screenPosition);

        if (point.Properties.IsLeftButtonPressed)
        {
            _editManipulation.Manipulate(MouseState.Dragging, screenPosition,
                _editManager, _mapControl);
        }
        else
        {
            _editManipulation.Manipulate(MouseState.Moving, screenPosition,
                _editManager, _mapControl);
        }
    }

    internal void MapControlOnPointerReleased(object? sender, PointerReleasedEventArgs args)
    {
        var point = args.GetCurrentPoint(_mapControl);

        if (_isRightWasPressed) // need for escape drawing by right click
        {
            _isRightWasPressed = false;
            return;
        }

        if (_mapControl.Map != null)
            _mapControl.Map.PanLock = _editManipulation.Manipulate(MouseState.Up,
                args.GetPosition(_mapControl).ToMapsui(), _editManager, _mapControl);
    }

    internal void MapControlOnPointerPressed(object? sender, PointerPressedEventArgs args)
    {
        var point = args.GetCurrentPoint(_mapControl);

        if (_mapControl.Map == null)
            return;

        if (point.Properties.IsRightButtonPressed)
        {
            _isRightWasPressed = true;
            var infoArgs = _mapControl.GetMapInfo(args.GetPosition(_mapControl).ToMapsui());
            FeatureUnderPointer = infoArgs?.Feature as BaseGraphic;
            return;
        }

        if (args.ClickCount > 1)
        {
            _mapControl.Map.PanLock = _editManipulation.Manipulate(MouseState.DoubleClick,
                args.GetPosition(_mapControl).ToMapsui(), _editManager, _mapControl);
            args.Handled = true;
        }
        else
        {
            _mapControl.Map.PanLock = _editManipulation.Manipulate(MouseState.Down,
                args.GetPosition(_mapControl).ToMapsui(), _editManager, _mapControl);
        }
    }

    private void DeleteGraphic()
    {
        if (FeatureUnderPointer is null)
            throw new NullReferenceException("Graphic was null");
        _editManager.Layer.TryRemove(FeatureUnderPointer);
    }

    private async Task LoadGraphicStateAsyncImpl()
    {
        var loadLocation = await ShowOpenFileDialogAsync.Handle(new List<string> { "txt" });
        if (loadLocation is null)
            return;
        var graphics = new List<BaseGraphic>();
        var isLoadSuccess = await BaseGraphicJsonMarshaller.TryLoadAsync(graphics, loadLocation);
        if (!isLoadSuccess)
        {
            ShowNotification(
                "Выбранный файл не удалось преобразовать в объекты", 
                "Ошибка",
                Colors.Red);
            return;
        }
        await LoadPointImagesAsync(graphics);
        if (_unregisteredImages.Any())
        {
            ShowImageNotification("Проблема загрузки изображений", "Информация", Colors.LightBlue);
            _unregisteredImages.Clear();
        }
        LoadGraphicsInLayer(graphics, loadLocation);
    }

    private async Task LoadPointImagesAsync(List<BaseGraphic> graphics)
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

    private void LoadGraphicsInLayer(List<BaseGraphic> newGraphics, string loadLocation)
    {
        _savedGraphicLayer!.Clear();
        _savedGraphicLayer.AddRange(newGraphics);
        LastFilePath = loadLocation;
        LoadedFileName = Path.GetFileName(loadLocation);
        _mapControl.Refresh();
    }        

    private async Task SaveGraphicStateInFileImpl()
    {
        var saveLocation = await ShowSaveGraphicStateDialog.Handle(Unit.Default);
        if (saveLocation is null)
            return;
        LastFilePath = saveLocation;
        await SaveGraphic(saveLocation);
    }

    private async Task SaveGraphic(string location)
    {
        await BaseGraphicJsonMarshaller.SaveAsync(_savedGraphicLayer!.Cast<BaseGraphic>(), location);
        LoadedFileName = Path.GetFileName(location);
    }
}