using Avalonia.Controls;
using Mapsui.UI.Avalonia;
using map_app.ViewModels;

namespace map_app.Views;

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