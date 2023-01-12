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
        var vm = new MainWindowViewModel(MapControl); // todo: remove mapcontrol dependency
        DataContext = vm;
        MapControl.PointerMoved += vm.MapControlOnPointerMoved;
        MapControl.PointerPressed += vm.MapControlOnPointerPressed;
        MapControl.PointerReleased += vm.MapControlOnPointerReleased;
        GraphicCotxtMenu.ContextMenuOpening += vm.AccessOnlyGraphic;
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