using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using map_app.Editing.Extensions;
using map_app.Models;
using map_app.Services;
using map_app.Services.Renders;
using Mapsui;
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
    private readonly Color LineColor = new Color(0, 0, 255, 100);
    private readonly IStyle _graphicLabels;
    private double _kilometerInterval;

    public AuxiliaryPanelViewModel(MainViewModel mainViewModel)
    {
        _mapControl = mainViewModel.MapControl;
        _mainViewModel = mainViewModel;
        _graphicLabels = (mainViewModel.Graphics.Style as StyleCollection)!.Styles.First(x => x is LabelDistanceStyle);
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
        IsRulerActivated ^= true;
        _graphicLabels.Enabled = IsRulerActivated;
    }

    private void ZoomIn() => _mapControl!.Navigator!.ZoomIn(200);

    private void ZoomOut() => _mapControl!.Navigator!.ZoomOut(200);
}