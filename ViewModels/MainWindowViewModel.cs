using System;
using System.Collections.Generic;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.UI.Avalonia;
using map_app.Services;
using map_app.Editing;
using System.Linq;
using Avalonia.Input;
using Mapsui.UI.Avalonia.Extensions;

namespace map_app.ViewModels
{    
    public class MainWindowViewModel
    {
        private WritableLayer? _targetLayer;
        private IEnumerable<IFeature>? _tempFeatures;
        private readonly EditManager _editManager = new();
        private readonly EditManipulation _editManipulation = new();
        private bool _selectMode;
        private bool _gridIsActive = true;
        private bool _leftWasPressed;

        private readonly MapControl MapControl;

        public MainWindowViewModel(MapControl mapControl)
        {
            MapControl = mapControl;
            MapControl.Map = MapCreator.Create();
            InitializeEditSetup();
        }

        private void Cancel()
        {
            if (_targetLayer != null && _tempFeatures != null)
            {
                _targetLayer.Clear(); 
                _targetLayer.AddRange(_tempFeatures.Copy());
                MapControl.RefreshGraphics();
            }

            _editManager.Layer?.Clear();

            MapControl.RefreshGraphics();

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

            MapControl.RefreshGraphics();
        }

        private void Save()
        {
            _targetLayer?.AddRange(_editManager.Layer?.GetFeatures().Copy() ?? new List<IFeature>());
            _editManager.Layer?.Clear();

            MapControl.RefreshGraphics();
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
                MapControl.RefreshGraphics();
            }
        }

        private void AddPoint() => AddShape(EditMode.AddPoint);

        private void AddLine() => AddShape(EditMode.AddLine);

        private void AddPolygon() => AddShape(EditMode.AddPolygon);

        private void AddRectangle() => AddShape(EditMode.AddRectangle);

        private void AddOrthodromeLine() => AddShape(EditMode.AddOrthodromeLine);

        private void AddShape(EditMode mode)
        {
            SaveBuffer();
            _editManager.EditMode = mode;
        }

        private void SaveBuffer()
        {
            var features = _targetLayer?.GetFeatures().Copy() ?? Array.Empty<IFeature>();
            foreach (var feature in features)
            {
                feature.RenderedGeometry.Clear();
            }
            _tempFeatures = new List<IFeature>(features);
        }

        private void Scale() => _editManager.EditMode = EditMode.Scale;

        private void Select() => _selectMode = !_selectMode;

        internal void MapControlOnPointerMoved(object? sender, PointerEventArgs args)
        {
            var point = args.GetCurrentPoint(MapControl);
            var screenPosition = args.GetPosition(MapControl).ToMapsui();
            var worldPosition = MapControl.Viewport.ScreenToWorld(screenPosition);

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
            var point = args.GetCurrentPoint(MapControl);

            if (!_leftWasPressed)
                return;

            if (MapControl.Map != null)
                MapControl.Map.PanLock = _editManipulation.Manipulate(MouseState.Up,
                    args.GetPosition(MapControl).ToMapsui(), _editManager, MapControl);

            if (_selectMode)
            {
                var infoArgs = MapControl.GetMapInfo(args.GetPosition(MapControl).ToMapsui());
                if (infoArgs?.Feature != null)
                {
                    var currentValue = (bool?)infoArgs.Feature["Selected"] == true;
                    infoArgs.Feature["Selected"] = !currentValue; // invert current value
                }
            }
        }

        internal void MapControlOnPointerPressed(object? sender, PointerPressedEventArgs args)
        {
            var point = args.GetCurrentPoint(MapControl);

            if (!point.Properties.IsLeftButtonPressed)
            {
                _leftWasPressed = false;
                return;
            }
            _leftWasPressed = true;
            if (MapControl.Map == null)
                return;

            if (args.ClickCount > 1)
            {
                MapControl.Map.PanLock = _editManipulation.Manipulate(MouseState.DoubleClick,
                    args.GetPosition(MapControl).ToMapsui(), _editManager, MapControl);
                args.Handled = true;
            }
            else
            {
                MapControl.Map.PanLock = _editManipulation.Manipulate(MouseState.Down,
                    args.GetPosition(MapControl).ToMapsui(), _editManager, MapControl);
            }
        }

        private void InitializeEditSetup()
        {
            _editManager.Layer = (WritableLayer)MapControl.Map!.Layers.First(l => l.Name == "EditLayer");
            _targetLayer = (WritableLayer)MapControl.Map.Layers.First(l => l.Name == "Layer 3");

            // Load the polygon layer on startup so you can start modifying right away
            _editManager.Layer.AddRange(_targetLayer.GetFeatures().Copy());
            _targetLayer.Clear();

            _editManager.EditMode = EditMode.Modify;
        }

        private void Modify() => _editManager.EditMode = EditMode.Modify;

        private void Rotate() => _editManager.EditMode = EditMode.Rotate;

        private void AddGridReference()
        {
            if (_gridIsActive)
            {

                MapControl.Map!.Layers.Add(GridReference.Grid);
                _gridIsActive = false;
                MapControl.RefreshGraphics();
                return;
            }

            MapControl.Map!.Layers.Remove(GridReference.Grid);
            MapControl.RefreshGraphics();
            _gridIsActive = true;
        }
    }
}