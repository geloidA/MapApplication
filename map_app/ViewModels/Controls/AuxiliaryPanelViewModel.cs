using System;
using System.ComponentModel.DataAnnotations;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using map_app.Services;
using Mapsui.Styles;
using Mapsui.UI.Avalonia;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace map_app.ViewModels.Controls;

public class AuxiliaryPanelViewModel : ViewModelBase
{
    private readonly GridLayer _gridLayer;
    private readonly MainViewModel _mainViewModel;
    private readonly GridMemoryProvider _gridLinesProvider;
    private readonly MapControl _mapControl;
    private readonly Color LineColor = new Color(0, 0, 255, 100);
    private double _kilometerInterval;

    public AuxiliaryPanelViewModel(MainViewModel mainViewModel)
    {
        _mapControl = mainViewModel.MapContrl;
        _mainViewModel = mainViewModel;
        _gridLinesProvider = new GridMemoryProvider(_mapControl.Viewport, this.WhenAnyValue(x => x.IsGridActivated));
        _mapControl.Navigator!.Navigated += (_, _) => KilometerInterval = _mapControl.Viewport.Resolution / 25;
        _gridLayer = new GridLayer(_gridLinesProvider)
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
    }

    private void ZoomIn() => _mapControl!.Navigator!.ZoomIn(200);

    private void ZoomOut() => _mapControl!.Navigator!.ZoomOut(200);        
}