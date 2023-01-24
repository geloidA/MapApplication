using System;
using Mapsui.Layers;
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

namespace map_app.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Private members
        private bool _isRightWasPressed;
        private readonly OwnWritableLayer? _savedGraphicLayer;
        private readonly MapControl _mapControl;
        private readonly EditManager _editManager = new();
        private readonly EditManipulation _editManipulation = new();
        private readonly ObservableAsPropertyHelper<bool> _isBaseGraphicUnderPointer;
        #endregion

        public bool IsBaseGraphicUnderPointer => _isBaseGraphicUnderPointer.Value;

        [Reactive]
        private BaseGraphic? FeatureUnderPointer { get; set; }

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
            _savedGraphicLayer = (OwnWritableLayer)_mapControl.Map!.Layers.First(l => l.Name == "Target Layer");
            _savedGraphicLayer.Clear();
            _editManager.Layer = _savedGraphicLayer; // todo: change on one layer
            GraphicsPopupViewModel = new GraphicsPopupViewModel(_savedGraphicLayer!);
            NavigationPanelViewModel = new NavigationPanelViewModel(mapControl, _editManager, _savedGraphicLayer!);
            AuxiliaryPanelViewModel = new AuxiliaryPanelViewModel(mapControl);
            ShowLayersManageDialog = new Interaction<LayersManageViewModel, MainViewModel>();
            OpenLayersManageView = ReactiveCommand.CreateFromTask(async () =>
            {
                var manager = new LayersManageViewModel(_mapControl.Map);
                var result = await ShowLayersManageDialog.Handle(manager);
            });

            _isBaseGraphicUnderPointer = this
                .WhenAnyValue(x => x.FeatureUnderPointer)
                .Select(f => f as BaseGraphic != null)
                .ToProperty(this, x => x.IsBaseGraphicUnderPointer);
        }

        public ICommand OpenLayersManageView { get; }

        public Interaction<LayersManageViewModel, MainViewModel> ShowLayersManageDialog { get; }

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
            _editManager.Layer?.TryRemove(FeatureUnderPointer);
        }
    }
}