using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using map_app.Models;
using map_app.Services;
using map_app.Services.Renders;
using Mapsui.Styles;
using Mapsui.UI.Avalonia;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace map_app.ViewModels.Controls;

public class AuxiliaryPanelViewModel : ViewModelBase
{
    private readonly MapGridLayer _gridLayer;
    private readonly MainViewModel _mainViewModel;
    private readonly GridMemoryProvider _gridLinesProvider;
    private readonly MapControl _mapControl;
    private readonly GraphicsLayer _graphicsLayer;
    private readonly Color LineColor = new Color(0, 0, 255, 100);
    private double _kilometerInterval;

    public AuxiliaryPanelViewModel(MainViewModel mainViewModel)
    {
        _mapControl = mainViewModel.MapControl;
        _graphicsLayer = (GraphicsLayer)_mapControl.Map!.Layers.FindLayer("Graphic Layer").Single();
        _mainViewModel = mainViewModel;
        _gridLinesProvider = new GridMemoryProvider(_mapControl.Viewport, this.WhenAnyValue(x => x.IsGridActivated));
        _mapControl.Navigator!.Navigated += (_, _) => KilometerInterval = _mapControl.Viewport.Resolution / 25;
        _gridLayer = new MapGridLayer(_gridLinesProvider)
        {
            Style = new VectorStyle { Line = new Pen(LineColor, 1) }
        };
        IsGridActivated = false;
    }

    [Range(0.1, 1000, ErrorMessage = "Пожалуйста введите число между 0.1 и 1000")]
    public double KilometerInterval 
    {
        get => _kilometerInterval;
        set
        {
            if (value < 0.1 || value > 1000)
                return;
            this.RaiseAndSetIfChanged(ref _kilometerInterval, value);
            _gridLinesProvider.KilometerInterval = _kilometerInterval;
        }
    }

    [Reactive]
    public bool IsGridActivated { get; set; }

    [Reactive]
    public bool IsRulerActivated { get; set; }

    private void ShowGridReference()
    {
        IsGridActivated ^= true;
        if (IsGridActivated)
        {
            _mapControl.Map!.Layers.Add(_gridLayer);
            return;
        }
        _mapControl.Map!.Layers.Remove(_gridLayer);
    }

    private void EnableRuler()
    {
        IsRulerActivated = (_mainViewModel.IsRulerActivated ^= true);
        // because layer styles draw first, distance labels must be in graphic feature for escape overlapping
        foreach (var graphic in _graphicsLayer.Features.Where(x => x is not PointGraphic))
        {
            var lableStyle = graphic.Styles.First(x => x is LabelDistanceStyle);
            lableStyle.Enabled = IsRulerActivated;
        }
        _mapControl.RefreshGraphics();
    }

    private void ZoomIn() => _mapControl!.Navigator!.ZoomIn(200);

    private void ZoomOut() => _mapControl!.Navigator!.ZoomOut(200);
}