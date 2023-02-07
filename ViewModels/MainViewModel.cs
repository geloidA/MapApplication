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
using Mapsui.Extensions;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets;

namespace map_app.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Private members
        private bool _isRightWasPressed;
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
            var scaleWidget = GetScaleWidget(_mapControl.Map);
            _mapControl.Map.Widgets.Add(scaleWidget);
            _savedGraphicLayer = (OwnWritableLayer)_mapControl.Map!.Layers.First(l => l.Name == "Graphic Layer");
            _savedGraphicLayer.Clear();
            _savedGraphicLayer.LayersFeatureChanged += (_, _) => HaveGraphics = _savedGraphicLayer.Any();
            _editManager = new EditManager(_savedGraphicLayer);
            _editManager.Extent = new Mapsui.MRect(LeftBorderMap, LeftBorderMap, -LeftBorderMap, -LeftBorderMap);
            GraphicsPopupViewModel = new GraphicsPopupViewModel(_savedGraphicLayer!);
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
            var canSave = this.WhenAnyValue(x => x.HaveGraphics);
            SaveGraphicStateInFile = ReactiveCommand.CreateFromTask(SaveGraphicStateInFileImpl, canSave);
            ShowOpenGraphicStateDialog = new Interaction<Unit, string?>();
            LoadGraphicState = ReactiveCommand.CreateFromTask(LoadGraphicStateImpl);
            var canSaveOpened = this
                .WhenAnyValue(x => x.LastFilePath)
                .Select(file => !string.IsNullOrEmpty(file));
            SaveGraphicStateInOpenedFile = ReactiveCommand.CreateFromTask(async () => await SaveGraphic(LastFilePath!), canSaveOpened);
        }

        public ICommand OpenLayersManageView { get; }

        public ICommand OpenGraphicEditingView { get; }

        public ICommand SaveGraphicStateInOpenedFile { get; }

        public ICommand SaveGraphicStateInFile { get; }

        public ICommand LoadGraphicState { get; }

        public Interaction<LayersManageViewModel, MainViewModel> ShowLayersManageDialog { get; }

        public Interaction<GraphicAddEditViewModel, MainViewModel> ShowGraphicEditingDialog { get; }

        public Interaction<Unit, string?> ShowSaveGraphicStateDialog { get; }
        
        public Interaction<Unit, string?> ShowOpenGraphicStateDialog { get; }

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

        private IWidget GetScaleWidget(Mapsui.Map map)
        {
            var scaleWidget = new ScaleBarWidget(map);
            scaleWidget.HorizontalAlignment = HorizontalAlignment.Right;
            scaleWidget.MarginX = 10F;
            scaleWidget.MarginY = 10F;
            return scaleWidget;
        }

        private void DeleteGraphic()
        {
            if (FeatureUnderPointer is null)
                throw new NullReferenceException("Graphic was null");
            _editManager.Layer.TryRemove(FeatureUnderPointer);
        }

        private async Task LoadGraphicStateImpl()
        {
            var loadLocation = await ShowOpenGraphicStateDialog.Handle(Unit.Default);
            if (loadLocation is null)
                return;
            _savedGraphicLayer!.Clear();
            await BaseGraphicJsonMarshaller.LoadAsync(_savedGraphicLayer, loadLocation);
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
}