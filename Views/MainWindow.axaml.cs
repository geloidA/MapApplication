using Avalonia.Controls;
using Mapsui.UI.Avalonia;
using map_app.ViewModels;
using Avalonia.ReactiveUI;
using System.Threading.Tasks;
using ReactiveUI;

namespace map_app.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        var dataContext = new MainWindowViewModel(MapControl); // todo: remove mapcontrol dependency
        DataContext = dataContext;
        MapControl.PointerMoved += dataContext.MapControlOnPointerMoved;
        MapControl.PointerPressed += dataContext.MapControlOnPointerPressed;
        MapControl.PointerReleased += dataContext.MapControlOnPointerReleased;
        this.WhenActivated(d => d(ViewModel!.ShowDialog.RegisterHandler(DoShowDialogAsync)));
    }

    private async Task DoShowDialogAsync(InteractionContext<LayersManageViewModel, MainWindowViewModel> interaction)
    {
        var dialog = new LayersManageView();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<MainWindowViewModel>(this);
        interaction.SetOutput(result);
    }
}