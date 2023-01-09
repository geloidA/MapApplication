using System;
using System.Collections.Generic;
using Mapsui;
using Mapsui.Extensions;
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
using map_app.Views;

namespace map_app.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private WritableLayer? _targetLayer;
        private IEnumerable<IFeature>? _tempFeatures;
        private readonly EditManager _editManager = new();
        private readonly EditManipulation _editManipulation = new();
        private bool _selectMode;
        private bool _gridIsActive = true;
        private bool _leftWasPressed;
        private readonly MapControl _mapControl;

        public MainWindowViewModel(MapControl mapControl)
        {
            _mapControl = mapControl;
            _mapControl.Map = MapCreator.Create();
            InitializeEditSetup();
            EnablePointMode = ReactiveCommand.Create(() => EnableDrawingMode(EditMode.AddPoint));
            EnablePolygonMode = ReactiveCommand.Create(() => EnableDrawingMode(EditMode.AddPolygon));
            EnableOrthodromeMode = ReactiveCommand.Create(() => EnableDrawingMode(EditMode.AddOrthodromeLine));
            EnableRectangleMode = ReactiveCommand.Create(() => EnableDrawingMode(EditMode.AddRectangle));
            ShowDialog = new Interaction<LayersManageViewModel, MainWindowViewModel>();
            OpenLayersManageView = ReactiveCommand.CreateFromTask(async () =>
            {
                var manager = new LayersManageViewModel(_mapControl.Map);
                var result = await ShowDialog.Handle(manager);
            });
        }

        public ICommand EnablePointMode { get; }
        public ICommand EnablePolygonMode { get; }
        public ICommand EnableOrthodromeMode { get; }
        public ICommand EnableRectangleMode { get; }
        public ICommand OpenLayersManageView { get; }
        public Interaction<LayersManageViewModel, MainWindowViewModel> ShowDialog { get; }

        private void InitializeEditSetup()
        {
            _editManager.Layer = (WritableLayer)_mapControl.Map!.Layers.First(l => l.Name == "EditLayer");
            _targetLayer = (WritableLayer)_mapControl.Map!.Layers.First(l => l.Name == "Target Layer");
            _targetLayer.Clear();
        }

        private void Cancel()
        {
            if (_targetLayer != null && _tempFeatures != null)
            {
                _targetLayer.Clear(); 
                _targetLayer.AddRange(_tempFeatures.Copy());
                _mapControl.RefreshGraphics();
            }

            _editManager.Layer?.Clear();

            _mapControl.RefreshGraphics();

            _editManager.EditMode = EditMode.None;

            _tempFeatures = null;
        }

        private void Load()
        {
            var features = _targetLayer?.GetFeatures().Copy() ?? Array.Empty<IFeature>();

            foreach (var feature in features)
            {
                feature.RenderedGeometry.Clear();
            }

            _tempFeatures = new List<IFeature>(features);

            _editManager.Layer?.AddRange(features);
            _targetLayer?.Clear();

            _mapControl.RefreshGraphics();
        }

        private void Save()
        {
            _targetLayer?.AddRange(_editManager.Layer?.GetFeatures().Copy() ?? new List<IFeature>());
            _editManager.Layer?.Clear();

            _mapControl.RefreshGraphics();
        }

        private void None() => _editManager.EditMode = EditMode.None;

        private void Delete()
        {
            if (_selectMode)
            {
                var selectedFeatures = _editManager.Layer?.GetFeatures().Where(f => (bool?)f["Selected"] == true) ?? Array.Empty<IFeature>();

                foreach (var selectedFeature in selectedFeatures)
                {
                    _editManager.Layer?.TryRemove(selectedFeature);
                }
                _mapControl.RefreshGraphics();
            }
        }

        private void Scale() => _editManager.EditMode = EditMode.Scale;

        private void Select() => _selectMode = !_selectMode;

        private void Modify() => _editManager.EditMode = EditMode.Modify;

        private void Rotate() => _editManager.EditMode = EditMode.Rotate;

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

            if (!_leftWasPressed)
                return;

            if (_mapControl.Map != null)
                _mapControl.Map.PanLock = _editManipulation.Manipulate(MouseState.Up,
                    args.GetPosition(_mapControl).ToMapsui(), _editManager, _mapControl);

            if (_selectMode)
            {
                var infoArgs = _mapControl.GetMapInfo(args.GetPosition(_mapControl).ToMapsui());
                if (infoArgs?.Feature != null)
                {
                    var currentValue = (bool?)infoArgs.Feature["Selected"] == true;
                    infoArgs.Feature["Selected"] = !currentValue; // invert current value
                }
            }
        }

        internal void MapControlOnPointerPressed(object? sender, PointerPressedEventArgs args)
        {
            var point = args.GetCurrentPoint(_mapControl);

            if (!point.Properties.IsLeftButtonPressed)
            {
                _leftWasPressed = false;
                return;
            }
            _leftWasPressed = true;
            if (_mapControl.Map == null)
                return;

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

        private void EnableDrawingMode(EditMode mode)
        {
            ClearAllFeatureRenders();
            _editManager.EditMode = mode;
        }

        private void ClearAllFeatureRenders()
        {
            var features = _targetLayer?.GetFeatures().Copy() ?? Array.Empty<IFeature>();
            foreach (var feature in features)
            {
                feature.RenderedGeometry.Clear();
            }
            _tempFeatures = new List<IFeature>(features);
        }

        #region AuxiliaryPanel Commands
        private void ShowGridReference()
        {
            if (_gridIsActive)
            {

                _mapControl.Map!.Layers.Add(GridReference.Grid);
                _gridIsActive = false;
                _mapControl.RefreshGraphics();
                return;
            }

            _mapControl.Map!.Layers.Remove(GridReference.Grid);
            _mapControl.RefreshGraphics();
            _gridIsActive = true;
        }

        private void EnableRuler()
        {

        }

        private void ZoomIn() => _mapControl!.Navigator!.ZoomIn(200);

        private void ZoomOut() => _mapControl!.Navigator!.ZoomOut(200);

        #endregion
    }
}