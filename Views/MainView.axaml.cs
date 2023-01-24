using Mapsui.UI.Avalonia;
using map_app.ViewModels;
using Avalonia.ReactiveUI;
using System.Threading.Tasks;
using ReactiveUI;
using map_app.ViewModels.Controls;

namespace map_app.Views;

public partial class MainView : ReactiveWindow<MainViewModel>
{
    public MainView()
    {
        InitializeComponent();
        var vm = new MainViewModel(MapControl); // todo: remove mapcontrol dependency
        DataContext = vm;
        MapControl.PointerMoved += vm.MapControlOnPointerMoved;
        MapControl.PointerPressed += vm.MapControlOnPointerPressed;
        MapControl.PointerReleased += vm.MapControlOnPointerReleased;
        GraphicCotxtMenu.ContextMenuOpening += vm.AccessOnlyGraphic;
        this.WhenActivated(d => d(ViewModel!.ShowLayersManageDialog.RegisterHandler(DoShowLayersManageDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.ShowGraphicEditingDialog.RegisterHandler(DoShowGraphicEditingDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.GraphicsPopupViewModel.ShowEditGraphicDialog.RegisterHandler(DoShowEditGraphicDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.GraphicsPopupViewModel.ShowAddGraphicDialog.RegisterHandler(DoShowAddGraphicDialogAsync)));
    }

    private async Task DoShowLayersManageDialogAsync(InteractionContext<LayersManageViewModel, MainViewModel> interaction)
    {
        var dialog = new LayersManageView();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<MainViewModel>(this);
        interaction.SetOutput(result);
    }

    private async Task DoShowGraphicEditingDialogAsync(InteractionContext<GraphicEditingViewModel, MainViewModel> interaction)
    {
        var dialog = new GraphicEditingView();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<MainViewModel>(this);
        interaction.SetOutput(result);
    }

    private async Task DoShowEditGraphicDialogAsync(InteractionContext<EditGraphicViewModel, GraphicsPopupViewModel> interaction)
    {
        var dialog = new EditGraphicView();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<GraphicsPopupViewModel>(this);
        interaction.SetOutput(result);
    }

    private async Task DoShowAddGraphicDialogAsync(InteractionContext<AddGraphicViewModel, GraphicsPopupViewModel> interaction)
    {
        var dialog = new AddGraphicView();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<GraphicsPopupViewModel>(this);
        interaction.SetOutput(result);
    }
}