using Avalonia.Media;
using Avalonia.Media.Immutable;
using map_app.Editing;
using map_app.Services.Attributes;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Linq;
using System.Windows.Input;
using AColor = Avalonia.Media.Color;
using MColor = Mapsui.Styles.Color;

namespace map_app.ViewModels.Controls;

internal class NavigationPanelViewModel : ViewModelBase
{
    private readonly string[] _modesNames;
    private readonly MainViewModel _mainVM;

    public NavigationPanelViewModel(MainViewModel mainViewModel)
    {
        _modesNames = GetModeNames();
        _mainVM = mainViewModel;
        EnablePointMode = ReactiveCommand.Create(() => SwitchDrawingMode(nameof(IsPointMode), EditMode.AddPoint));
        EnablePolygonMode = ReactiveCommand.Create(() => SwitchDrawingMode(nameof(IsPolygonMode), EditMode.AddPolygon));
        EnableOrthodromeMode = ReactiveCommand.Create(() => SwitchDrawingMode(nameof(IsOrthodromeMode), EditMode.AddOrthodromeLine));
        EnableRectangleMode = ReactiveCommand.Create(() => SwitchDrawingMode(nameof(IsRectangleMode), EditMode.AddRectangle));
        EnableDragMode = ReactiveCommand.Create(() => SwitchDrawingMode(nameof(IsDragMode), EditMode.Drag));

        ChooseColor = ReactiveCommand.Create<ImmutableSolidColorBrush>(brush => CurrentColor = brush.Color);
        this.WhenAnyValue(x => x.CurrentColor)
            .Subscribe(c => _mainVM.EditManagerColor = new MColor(c.R, c.G, c.B, c.A));
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
    public bool IsDragMode { get; set; }

    [Reactive]
    public AColor CurrentColor { get; set; } = Colors.Gray;

    public ICommand EnablePointMode { get; }

    public ICommand EnablePolygonMode { get; }

    public ICommand EnableOrthodromeMode { get; }

    public ICommand EnableRectangleMode { get; }

    public ICommand EnableDragMode { get; }

    public ICommand ChooseColor { get; }

    private void SwitchDrawingMode(string modeName, EditMode editMode)
    {
        var type = GetType();
        var drawingMode = type.GetProperty(modeName);
        var isSwitchedModeOn = !(bool)drawingMode?.GetValue(this)! ^ true;
        drawingMode?.SetValue(this, isSwitchedModeOn);

        foreach (var mode in _modesNames.Where(x => x != modeName))
            TurnOffMode(type, mode);

        if (_mainVM.EditMode != EditMode.None && EditMode.DrawingMode.HasFlag(_mainVM.EditMode))
            _mainVM.EndIncompleteEditing();
        _mainVM.EditMode = isSwitchedModeOn ? editMode : EditMode.None;
    }

    private void TurnOffMode(Type type, string modeName) => type.GetProperty(modeName)?.SetValue(this, false);

    private static string[] GetModeNames()
    {
        return typeof(NavigationPanelViewModel)
            .GetProperties()
            .Where(x => x.CustomAttributes.Any(a => a.AttributeType == typeof(ModeAttribute)))
            .Select(x => x.Name)
            .ToArray();
    }
}