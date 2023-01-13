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
using map_app.Models;
using ReactiveUI.Fody.Helpers;
using System.ComponentModel;
using Avalonia.Svg;
using Avalonia.Controls;
using DynamicData;
using System.Collections.ObjectModel;

namespace map_app.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private static Image _arrowRight = new Image
        {
            Source = new SvgImage
            {
                Source = SvgSource.Load("Resources/Assets/triangle-right-small.svg", null)
            },
            Width = 15,
            Height = 15
        };
        private static Image _arrowLeft = new Image
        {
            Source = new SvgImage
            {
                Source = SvgSource.Load("Resources/Assets/triangle-left-small.svg", null)
            },
            Width = 15,
            Height = 15
        };

        #region Private members
        private bool _gridIsActive = true;
        private bool _isRightWasPressed;
        private OwnWritableLayer? _savedGraphicLayer; // todo: remove command
        private IEnumerable<IFeature>? _tempFeatures;
        private readonly MapControl _mapControl;
        private readonly EditManager _editManager = new();
        private readonly EditManipulation _editManipulation = new();
        private readonly ObservableAsPropertyHelper<bool> _isBaseGraphicUnder;
        private readonly ObservableAsPropertyHelper<Image> _popupArrow;
        #endregion

        public bool IsBaseGraphicUnder => _isBaseGraphicUnder.Value;
        public Image PopupArrow => _popupArrow.Value;

        [Reactive]
        private BaseGraphic? FeatureUnderPointer { get; set; }

        public MainViewModel(MapControl mapControl)
        {
            _mapControl = mapControl;
            _mapControl.Map = MapCreator.Create();
            InitializeEditSetup();
            var canEdit = this.WhenAnyValue(x => x.IsEditMode);
            EnablePointMode = ReactiveCommand.Create(() => EnableDrawingMode(EditMode.AddPoint), canEdit);
            EnablePolygonMode = ReactiveCommand.Create(() => EnableDrawingMode(EditMode.AddPolygon), canEdit);
            EnableOrthodromeMode = ReactiveCommand.Create(() => EnableDrawingMode(EditMode.AddOrthodromeLine), canEdit);
            EnableRectangleMode = ReactiveCommand.Create(() => EnableDrawingMode(EditMode.AddRectangle), canEdit);
            IsGraphicsListPressed = ReactiveCommand.Create(() => IsGraphicsListOpen ^= true);
            ShowDialog = new Interaction<LayersManageViewModel, MainViewModel>();
            OpenLayersManageView = ReactiveCommand.CreateFromTask(async () =>
            {
                var manager = new LayersManageViewModel(_mapControl.Map);
                var result = await ShowDialog.Handle(manager);
            });

            _isBaseGraphicUnder = this
                .WhenAnyValue(x => x.FeatureUnderPointer)
                .Select(f => f as BaseGraphic != null)
                .ToProperty(this, x => x.IsBaseGraphicUnder);

            _popupArrow = this
                .WhenAnyValue(x => x.IsGraphicsListOpen)
                .Select(isOpen => isOpen ? _arrowLeft : _arrowRight)
                .ToProperty(this, x => x.PopupArrow);

            this.WhenAnyValue(x => x.IsEditMode) // comboBox for edit mode
                .Subscribe(isEdit => 
                {
                    if (!isEdit)
                    {
                        None();
                        Save();
                    }
                    else
                        Load();
                });
            Graphics = new ObservableCollection<BaseGraphic>(GetSavedGraphics);
            _savedGraphicLayer!.LayersFeatureChanged += (s, e) => 
            {
                Graphics.Clear();
                Graphics?.AddRange(GetSavedGraphics);
            };
        }

        public ObservableCollection<BaseGraphic> Graphics { get; }

        [Reactive]
        public bool IsEditMode { get; set; } = true;

        [Reactive]
        public bool IsGraphicsListOpen { get; set; }

        public ICommand IsGraphicsListPressed { get; }
        public ICommand RemoveGraphic { get; }
        public ICommand EnablePointMode { get; }
        public ICommand EnablePolygonMode { get; }
        public ICommand EnableOrthodromeMode { get; }
        public ICommand EnableRectangleMode { get; }
        public ICommand OpenLayersManageView { get; }
        public Interaction<LayersManageViewModel, MainViewModel> ShowDialog { get; }

        internal void AccessOnlyGraphic(object? sender, CancelEventArgs e) => e.Cancel = !IsEditMode || !IsBaseGraphicUnder;

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

        private void InitializeEditSetup()
        {
            _editManager.Layer = (WritableLayer)_mapControl.Map!.Layers.First(l => l.Name == "EditLayer");
            _savedGraphicLayer = (OwnWritableLayer)_mapControl.Map!.Layers.First(l => l.Name == "Target Layer");
            _savedGraphicLayer.Clear();
        }

        private IEnumerable<BaseGraphic> GetSavedGraphics => _savedGraphicLayer!
                .GetFeatures()
                .Cast<BaseGraphic>();

        private void Cancel()
        {
            if (_savedGraphicLayer != null && _tempFeatures != null)
            {
                _savedGraphicLayer.Clear(); 
                _savedGraphicLayer.AddRange(_tempFeatures.Copy());
                _mapControl.RefreshGraphics();
            }
            _editManager.Layer?.Clear();
            _mapControl.RefreshGraphics();
            _editManager.EditMode = EditMode.None;
            _tempFeatures = null;
        }

        private void Load()
        {
            var features = _savedGraphicLayer?.GetFeatures().Copy() ?? Array.Empty<IFeature>();

            foreach (var feature in features)
            {
                feature.RenderedGeometry.Clear();
            }

            _tempFeatures = new List<IFeature>(features);

            _editManager.Layer?.AddRange(features);
            _savedGraphicLayer?.Clear();

            _mapControl.RefreshGraphics();
        }

        private void Save()
        {
            _savedGraphicLayer?.AddRange(_editManager.Layer?.GetFeatures().Copy() ?? new List<IFeature>());
            _editManager.Layer?.Clear();

            _mapControl.RefreshGraphics();
        }

        private void None() => _editManager.EditMode = EditMode.None;

        private void DeleteGraphic()
        {
            if (FeatureUnderPointer is null)
                throw new NullReferenceException("Graphic was null");
            _editManager?.Layer?.TryRemove(FeatureUnderPointer);
        }

        private void EnableDrawingMode(EditMode mode)
        {
            ClearAllFeatureRenders();
            _editManager.EditMode = mode;
        }

        private void ClearAllFeatureRenders()
        {
            var features = _savedGraphicLayer?.GetFeatures().Copy() ?? Array.Empty<IFeature>();
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
                return;
            }

            _mapControl.Map!.Layers.Remove(GridReference.Grid);
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