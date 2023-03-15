using Mapsui.UI.Avalonia;
using map_app.ViewModels;
using Avalonia.ReactiveUI;
using System.Threading.Tasks;
using ReactiveUI;
using System.Reactive;
using Avalonia.Controls;
using System.Linq;
using System.Collections.Generic;
using map_app.Services.Renders;
using Avalonia.Input;
using map_app.Services;

namespace map_app.Views;

public partial class MainView : ReactiveWindow<MainViewModel>
{
    public MainView()
    {
        InitializeComponent();
        MapControl.Renderer.StyleRenderers.Add(typeof(LabelDistanceStyle), new LabelDistanceStyleRenderer());
        var vm = new MainViewModel(MapControl); // todo: remove mapcontrol dependency
        DataContext = vm;
        MapControl.PointerMoved += vm.MapControlOnPointerMoved;
        MapControl.PointerPressed += vm.MapControlOnPointerPressed;
        MapControl.PointerReleased += vm.MapControlOnPointerReleased;
        GraphicCotxtMenu.ContextMenuOpening += vm.AccessOnlyGraphic;
        this.WhenActivated(d => d(ViewModel!.ShowLayersManageDialog.RegisterHandler(DoShowLayersManageDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.ShowGraphicEditingDialog.RegisterHandler(DoShowGraphicAddEditDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.GraphicsPopupViewModel.ShowAddEditGraphicDialog.RegisterHandler(DoShowGraphicAddEditDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.ShowSaveGraphicStateDialog.RegisterHandler(DoShowSaveGraphicStateDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.ShowOpenFileDialogAsync.RegisterHandler(DoShowOpenFileDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.ShowImportImagesDialogAsync.RegisterHandler(DoImportImagesDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.ShowSettingsDialog.RegisterHandler(DoShowSettingsDialogAsync)));
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            MapControl.RefreshGraphics();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            ViewModel?.EditManager.CancelDrawing();
    }

    private async Task DoShowLayersManageDialogAsync(InteractionContext<LayersManageViewModel, Unit> interaction) // unit means that we don't care about interaction result
    {
        var dialog = new LayersManageView { DataContext = interaction.Input };
        interaction.SetOutput(await dialog.ShowDialog<Unit>(this));
    }

    private async Task DoShowSettingsDialogAsync(InteractionContext<SettingsViewModel, Unit> interaction)
    {
        var dialog = new SettingsView { DataContext = interaction.Input };
        interaction.SetOutput(await dialog.ShowDialog<Unit>(this));
    }

    private async Task DoShowGraphicAddEditDialogAsync(InteractionContext<GraphicAddEditViewModel, Unit> interaction)
    {
        var dialog = new GraphicAddEditView { DataContext = interaction.Input };
        interaction.SetOutput(await dialog.ShowDialog<Unit>(this));
    }

    private async Task DoShowSaveGraphicStateDialogAsync(InteractionContext<MapStateSaveViewModel, MapState?> interaction)
    {
        var dialog = new MapStateSaveView { DataContext = interaction.Input };
        interaction.SetOutput(await dialog.ShowDialog<MapState?>(this));
    }

    private async Task DoShowOpenFileDialogAsync(InteractionContext<List<string>, string?> interaction)
    {
        var dialog = new OpenFileDialog
        {
            Filters = new List<FileDialogFilter>
            { 
                new FileDialogFilter { Extensions = interaction.Input }
            }
        };
        interaction.SetOutput((await dialog.ShowAsync(this))?.First());
    }

    private async Task DoImportImagesDialogAsync(InteractionContext<Unit, string[]?> interaction)
    {
        var dialog = new OpenFileDialog
        {
            AllowMultiple = true,
            Filters = new List<FileDialogFilter>
            { 
                new FileDialogFilter
                { 
                    Extensions = new List<string>
                    { 
                        "png", "webp", "jpg", "jpeg"
                    } 
                }
            }
        };
        interaction.SetOutput(await dialog.ShowAsync(this));
    }
}