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
using System;
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
        this.WhenActivated(d => d(ViewModel!.ShowLayersManageDialog.RegisterHandler(x => this.ShowDialogAsync(x, new LayersManageView()))));
        Func<InteractionContext<GraphicAddEditViewModel, DialogResult>, Task> graphicAddEditDialog = 
            x => this.ShowDialogAsync(x, new GraphicAddEditView());
        this.WhenActivated(d => d(ViewModel!.ShowGraphicEditingDialog.RegisterHandler(graphicAddEditDialog)));
        this.WhenActivated(d => d(ViewModel!.GraphicsPopupViewModel.ShowAddEditGraphicDialog.RegisterHandler(graphicAddEditDialog)));
        this.WhenActivated(d => d(ViewModel!.ShowSaveGraphicStateDialog.RegisterHandler(x => this.ShowDialogAsync(x, new MapStateSaveView()))));
        this.WhenActivated(d => d(ViewModel!.ShowSettingsDialog.RegisterHandler(x => this.ShowDialogAsync(x, new SettingsView()))));
        this.WhenActivated(d => d(ViewModel!.ShowOpenFileDialogAsync.RegisterHandler(DoShowOpenFileDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.ShowImportImagesDialogAsync.RegisterHandler(DoImportImagesDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.ShowExportOrhodromeIntervalsDialogAsync.RegisterHandler(x => this.ShowDialogAsync(x, new ExportOrhodromeIntervalsView()))));
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            ViewModel?.CancelDrawing();
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