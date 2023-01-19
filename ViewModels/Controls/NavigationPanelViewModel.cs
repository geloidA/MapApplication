using System;
using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using map_app.Editing;
using map_app.Services;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.UI.Avalonia;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace map_app.ViewModels.Controls
{
    internal class NavigationPanelViewModel : ViewModelBase
    {
        private OwnWritableLayer? _savedGraphicLayer;
        private IEnumerable<IFeature>? _tempFeatures;
        private readonly EditManager _editManager;
        private readonly MapControl _mapControl;
        
        public NavigationPanelViewModel(MapControl mapControl, 
            EditManager editManager, 
            OwnWritableLayer savedGraphicLayer)
        {
            _mapControl = mapControl;
            _editManager = editManager;
            _savedGraphicLayer = savedGraphicLayer;
            var canEdit = this.WhenAnyValue(x => x.IsEditMode);
            EnablePointMode = ReactiveCommand.Create(() => EnableDrawingMode(EditMode.AddPoint), canEdit);
            EnablePolygonMode = ReactiveCommand.Create(() => EnableDrawingMode(EditMode.AddPolygon), canEdit);
            EnableOrthodromeMode = ReactiveCommand.Create(() => EnableDrawingMode(EditMode.AddOrthodromeLine), canEdit);
            EnableRectangleMode = ReactiveCommand.Create(() => EnableDrawingMode(EditMode.AddRectangle), canEdit);
            
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

            ChooseColor = ReactiveCommand.Create<ImmutableSolidColorBrush>(brush => CurrentColor = brush.Color, canEdit);
            this.WhenAnyValue(x => x.CurrentColor)
                .Subscribe(c => _editManager.CurrentColor = new Mapsui.Styles.Color(c.R, c.G, c.B, c.A));
        }

        [Reactive]
        public bool IsEditMode { get; set; } = true;

        [Reactive]
        public Color CurrentColor { get; set; } = Colors.Gray;

        public ICommand EnablePointMode { get; }

        public ICommand EnablePolygonMode { get; }

        public ICommand EnableOrthodromeMode { get; }

        public ICommand EnableRectangleMode { get; }

        public ICommand ChooseColor { get; }

        private void Load()
        {
            var features = _savedGraphicLayer?.GetFeatures().Copy() ?? Array.Empty<IFeature>();

            foreach (var feature in features)
                feature.RenderedGeometry.Clear();

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
    }
}