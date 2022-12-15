using Avalonia.Controls;
using Mapsui.UI.Avalonia;
using Mapsui.Tiling.Layers;
using BruTile.Web;
using Mapsui;
using Mapsui.Extensions;
using BruTile.Predefined;
using Mapsui.Layers;
using Mapsui.Styles.Thematics;
using Mapsui.Styles;
using Mapsui.Layers.AnimatedLayers;
using System.Collections.Generic;
using Mapsui.Nts.Extensions;
using Mapsui.Providers;
using NetTopologySuite.Geometries;
using System;
using Mapsui.Nts;
using Avalonia.ReactiveUI;
using MapEditing;
using Avalonia.Interactivity;
using System.Linq;
using Avalonia.Input;
using Mapsui.UI.Avalonia.Extensions;
using Avalonia.Media;

namespace map_app;

public partial class MainWindow : Window
{    
    public MainWindow()
    {
        InitializeComponent();
        var dataContext = new MainWindowViewModel(MapControl);
        DataContext = dataContext;
        MapControl.PointerMoved += dataContext.MapControlOnPointerMoved;
        MapControl.PointerPressed += dataContext.MapControlOnPointerPressed;
        MapControl.PointerReleased += dataContext.MapControlOnPointerReleased;
    }
}