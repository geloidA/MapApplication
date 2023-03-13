using System;
using System.Linq;
using System.Windows.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using map_app.Editing;
using map_app.Services;
using Mapsui.UI.Avalonia;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using MColor = Mapsui.Styles.Color;
using AColor = Avalonia.Media.Color;

namespace map_app.ViewModels.Controls;

internal class NavigationPanelViewModel : ViewModelBase
{
    private readonly string[] _modesNames;
    private readonly GraphicsLayer _graphics;
    private readonly EditManager _editManager;
    private readonly MapControl _mapControl;
    
    public NavigationPanelViewModel(MainViewModel mainViewModel)
    {
        _modesNames = GetModeNames();
        _mapControl = mainViewModel.MapControl;
        _editManager = mainViewModel.EditManager;
        _graphics = mainViewModel.Graphics;
        EnablePointMode = ReactiveCommand.Create(() => SwitchDrawingMode(nameof(IsPointMode), EditMode.AddPoint));
        EnablePolygonMode = ReactiveCommand.Create(() => SwitchDrawingMode(nameof(IsPolygonMode), EditMode.AddPolygon));
        EnableOrthodromeMode = ReactiveCommand.Create(() => SwitchDrawingMode(nameof(IsOrthodromeMode), EditMode.AddOrthodromeLine));
        EnableRectangleMode = ReactiveCommand.Create(() => SwitchDrawingMode(nameof(IsRectangleMode), EditMode.AddRectangle));
        EnableModifyMode = ReactiveCommand.Create(() => SwitchDrawingMode(nameof(IsModifyMode), EditMode.Modify));

        ChooseColor = ReactiveCommand.Create<ImmutableSolidColorBrush>(brush => CurrentColor = brush.Color);
        this.WhenAnyValue(x => x.CurrentColor)
            .Subscribe(c => _editManager.CurrentColor = new MColor(c.R, c.G, c.B, c.A));
    }

    [Mode]
    [Reactive]
    public bool IsPointMode { get; set; }

    [Mode]
    [Reactive]
    public bool IsRectangleMode { get; set; }

    [Mode]
    [Reactive]
    public bool IsPolygonMode { get; set; }

    [Mode]
    [Reactive]
    public bool IsOrthodromeMode { get; set; }

    [Mode]
    [Reactive]
    public bool IsModifyMode { get; set; }

    [Reactive]
    public AColor CurrentColor { get; set; } = Colors.Gray;

    public ICommand EnablePointMode { get; }

    public ICommand EnablePolygonMode { get; }

    public ICommand EnableOrthodromeMode { get; }

    public ICommand EnableRectangleMode { get; }

    public ICommand EnableModifyMode { get; }

    public ICommand ChooseColor { get; }

    private static void ClearGraphicsLayerRenderedGeometry(GraphicsLayer layer)
    {
        foreach (var feature in layer.Features)
            feature.RenderedGeometry.Clear();
    }

    private void SwitchDrawingMode(string modeName, EditMode editMode)
    {
        var drawingMode = this.GetType().GetProperty(modeName);
        var isCurrentModeOn = !(bool)drawingMode?.GetValue(this)! ^ true;
        drawingMode?.SetValue(this, isCurrentModeOn);
        
        foreach (var mode in _modesNames.Where(x => x != modeName))
        {
            var prop = this.GetType().GetProperty(mode);
            prop?.SetValue(this, false);
        }
        if (isCurrentModeOn) ClearGraphicsLayerRenderedGeometry(_graphics);
        if (_editManager.HaveHoverVertex)
            _editManager.EndIncompleteEditing();
        _editManager.EditMode = isCurrentModeOn ? editMode : EditMode.None;
    }

    private static string[] GetModeNames()
    {
        return typeof(NavigationPanelViewModel)
            .GetProperties()
            .Where(x => x.CustomAttributes.Any(a => a.AttributeType == typeof(ModeAttribute)))
            .Select(x => x.Name)
            .ToArray();
    }
}