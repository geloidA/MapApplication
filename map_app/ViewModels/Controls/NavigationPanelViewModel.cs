using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using map_app.Editing;
using map_app.Models;
using map_app.Services;
using map_app.Services.Extensions;
using Mapsui;
using Mapsui.Styles;
using Mapsui.UI.Avalonia;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using MColor = Mapsui.Styles.Color;
using AColor = Avalonia.Media.Color;

namespace map_app.ViewModels.Controls;

internal class NavigationPanelViewModel : ViewModelBase
{
    private readonly OwnWritableLayer _graphics;
    private readonly EditManager _editManager;
    private readonly MapControl _mapControl;

    private readonly Mapsui.Styles.Pen EditOutlineStyle = new(Mapsui.Styles.Color.Red, 3);
    private readonly Mapsui.Styles.Pen EditLineStyle = new(Mapsui.Styles.Color.Red, 3);
    
    public NavigationPanelViewModel(MainViewModel mainViewModel)
    {
        _mapControl = mainViewModel.MapControl;
        _editManager = mainViewModel.EditManager;
        _graphics = mainViewModel.Graphics;
        var canEdit = this.WhenAnyValue(x => x.IsEditMode);
        EnablePointMode = ReactiveCommand.Create(() => EnableDrawingMode(EditMode.AddPoint), canEdit);
        EnablePolygonMode = ReactiveCommand.Create(() => EnableDrawingMode(EditMode.AddPolygon), canEdit);
        EnableOrthodromeMode = ReactiveCommand.Create(() => EnableDrawingMode(EditMode.AddOrthodromeLine), canEdit);
        EnableRectangleMode = ReactiveCommand.Create(() => EnableDrawingMode(EditMode.AddRectangle), canEdit);
        
        this.WhenAnyValue(x => x.IsEditMode) // comboBox for edit mode
            .Subscribe(isEdit => 
            {
                if (!isEdit)
                    None();
                ChangeFeaturesBorderLine();
            });

        ChooseColor = ReactiveCommand.Create<ImmutableSolidColorBrush>(brush => CurrentColor = brush.Color, canEdit);
        this.WhenAnyValue(x => x.CurrentColor)
            .Subscribe(c => _editManager.CurrentColor = new MColor(c.R, c.G, c.B, c.A));
    }

    [Reactive]
    public bool IsEditMode { get; set; } = true;

    [Reactive]
    public AColor CurrentColor { get; set; } = Colors.Gray;

    public ICommand EnablePointMode { get; }

    public ICommand EnablePolygonMode { get; }

    public ICommand EnableOrthodromeMode { get; }

    public ICommand EnableRectangleMode { get; }

    public ICommand ChooseColor { get; }

    private void ChangeFeaturesBorderLine()
    {
        if (_graphics.Style is not VectorStyle style)
            return;

        if (IsEditMode)
        {                
            style.Outline = EditOutlineStyle;
            style.Line = EditLineStyle;
        }
        else
        {
            style.Outline = null;
            style.Line = null;
        }
    }

    private void None() => _editManager.EditMode = EditMode.None;

    private void EnableDrawingMode(EditMode mode)
    {
        GetClearedSavedFeatures(_graphics);
        _editManager.EditMode = mode;
    }

    private static IEnumerable<IFeature> GetClearedSavedFeatures(OwnWritableLayer layer)
    {
        var features = layer.GetFeatures()
            .Cast<BaseGraphic>()
            .Copy() ?? Array.Empty<IFeature>();
        foreach (var feature in features)
            feature.RenderedGeometry.Clear();
        return features;
    }
    
    private void OnModify()
    {
        _editManager.EditMode = EditMode.Modify;
    }
}