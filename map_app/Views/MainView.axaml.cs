using Mapsui.UI.Avalonia;
using map_app.ViewModels;
using Avalonia.ReactiveUI;
using System.Threading.Tasks;
using ReactiveUI;
using map_app.ViewModels.Controls;
using System.Reactive;
using Avalonia.Controls;
using System.Linq;
using System;
using System.IO;
using System.Collections.Generic;

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
        this.WhenActivated(d => d(ViewModel!.GraphicsPopupViewModel.ShowAddEditGraphicDialog.RegisterHandler(DoShowGraphicAddEditDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.ShowSaveGraphicStateDialog.RegisterHandler(DoShowSaveGraphicStateDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.ShowOpenFileDialogAsync.RegisterHandler(DoShowOpenFileDialogAsync)));
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        var sessionUserImages = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "SessionUserImages"));
        foreach (var file in sessionUserImages.GetFiles())
            file.Delete();
    }

    private async Task DoShowLayersManageDialogAsync(InteractionContext<LayersManageViewModel, MainViewModel> interaction)
    {
        var dialog = new LayersManageView();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<MainViewModel>(this);
        interaction.SetOutput(result);
    }

    private async Task DoShowGraphicEditingDialogAsync(InteractionContext<GraphicAddEditViewModel, MainViewModel> interaction)
    {
        var dialog = new GraphicAddEditView();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<MainViewModel>(this);
        interaction.SetOutput(result);
    }

    private async Task DoShowGraphicAddEditDialogAsync(InteractionContext<GraphicAddEditViewModel, GraphicsPopupViewModel> interaction)
    {
        var dialog = new GraphicAddEditView();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<GraphicsPopupViewModel>(this);
        interaction.SetOutput(result);
    }

    private async Task DoShowSaveGraphicStateDialogAsync(InteractionContext<Unit, string?> interaction)
    {
        var dialog = new SaveFileDialog();
        dialog.DefaultExtension = ".txt";
        var result = await dialog.ShowAsync(this);

        interaction.SetOutput(result);
    }

    private async Task DoShowOpenFileDialogAsync(InteractionContext<List<string>, string?> interaction)
    {
        var dialog = new OpenFileDialog();
        dialog.Filters = new List<FileDialogFilter>
        { 
            new FileDialogFilter() { Extensions = interaction.Input }
        };
        var result = await dialog.ShowAsync(this);

        interaction.SetOutput(result?.First());
    }
}