using Avalonia.Controls;
using Mapsui.UI.Avalonia;
using map_app.ViewModels;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace map_app.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        this.WhenActivated(disposables => { });
        InitializeComponent();
        var dataContext = new MainWindowViewModel(MapControl);
        DataContext = dataContext;
        MapControl.PointerMoved += dataContext.MapControlOnPointerMoved;
        MapControl.PointerPressed += dataContext.MapControlOnPointerPressed;
        MapControl.PointerReleased += dataContext.MapControlOnPointerReleased;
    }
}