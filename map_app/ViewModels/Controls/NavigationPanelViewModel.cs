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
    private readonly string[] _modesNames = new[]
    {
        nameof(IsPointMode),
        nameof(IsOrthodromeMode),
        nameof(IsPolygonMode),
        nameof(IsRectangleMode)
    };
    private readonly GraphicsLayer _graphics;
    private readonly EditManager _editManager;
    private readonly MapControl _mapControl;

    private readonly Mapsui.Styles.Pen EditOutlineStyle = new(Mapsui.Styles.Color.Red, 3);
    private readonly Mapsui.Styles.Pen EditLineStyle = new(Mapsui.Styles.Color.Red, 3);
    
    public NavigationPanelViewModel(MainViewModel mainViewModel)
    {
        _mapControl = mainViewModel.MapControl;
        _editManager = mainViewModel.EditManager;
        _graphics = mainViewModel.Graphics;
        EnablePointMode = ReactiveCommand.Create(() => SwitchDrawingMode(nameof(IsPointMode), EditMode.AddPoint));
        EnablePolygonMode = ReactiveCommand.Create(() => SwitchDrawingMode(nameof(IsPolygonMode), EditMode.AddPolygon));
        EnableOrthodromeMode = ReactiveCommand.Create(() => SwitchDrawingMode(nameof(IsOrthodromeMode), EditMode.AddOrthodromeLine));
        EnableRectangleMode = ReactiveCommand.Create(() => SwitchDrawingMode(nameof(IsRectangleMode), EditMode.AddRectangle));

        ChooseColor = ReactiveCommand.Create<ImmutableSolidColorBrush>(brush => CurrentColor = brush.Color);
        this.WhenAnyValue(x => x.CurrentColor)
            .Subscribe(c => _editManager.CurrentColor = new MColor(c.R, c.G, c.B, c.A));
    }

    [Reactive]
    public bool IsPointMode { get; set; }

    [Reactive]
    public bool IsRectangleMode { get; set; }

    [Reactive]
    public bool IsPolygonMode { get; set; }

    [Reactive]
    public bool IsOrthodromeMode { get; set; }

    [Reactive]
    public AColor CurrentColor { get; set; } = Colors.Gray;

    public ICommand EnablePointMode { get; }

    public ICommand EnablePolygonMode { get; }

    public ICommand EnableOrthodromeMode { get; }

    public ICommand EnableRectangleMode { get; }

    public ICommand ChooseColor { get; }

    private void None() => _editManager.EditMode = EditMode.None;

    private void EnableDrawingMode(EditMode mode)
    {
        ClearGraphicsLayerRenderedGeometry(_graphics);
        _editManager.EditMode = mode;
    }

    private static void ClearGraphicsLayerRenderedGeometry(GraphicsLayer layer)
    {
        foreach (var feature in layer.Features)
            feature.RenderedGeometry.Clear();
    }

    private void SwitchDrawingMode(string modeName, EditMode editMode)
    {
        var toSwitch = this.GetType().GetProperty(modeName);
        var isTurnOn = !(bool)toSwitch?.GetValue(this)! ^ true;
        toSwitch?.SetValue(this, isTurnOn);
        
        foreach (var mode in _modesNames.Where(x => x != modeName))
        {
            var prop = this.GetType().GetProperty(mode);
            prop?.SetValue(this, false);
        }
        if (isTurnOn) ClearGraphicsLayerRenderedGeometry(_graphics);
        _editManager.EditMode = isTurnOn ? editMode : EditMode.None;
    }
    
    private void OnModify()
    {
        _editManager.EditMode = EditMode.Modify;
    }
}