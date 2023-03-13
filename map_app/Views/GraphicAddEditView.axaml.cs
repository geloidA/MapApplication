using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using map_app.ViewModels;
using ReactiveUI;

namespace map_app.Views;

public partial class GraphicAddEditView : ReactiveWindow<GraphicAddEditViewModel>
{
    public GraphicAddEditView()
    {
        InitializeComponent();
        this.WhenActivated(d => d(ViewModel!.ShowOpenFileDialog.RegisterHandler(DoShowOpenFileDialogAsync)));
    }

    private async Task DoShowOpenFileDialogAsync(InteractionContext<List<string>, string?> interaction)
    {
        var dialog = new OpenFileDialog
        {
            Directory = App.ImportImagesLocation,
            Filters = new List<FileDialogFilter>
            {
                new FileDialogFilter { Extensions = interaction.Input }
            }
        };
        var path = await dialog.ShowAsync(this);
        interaction.SetOutput(path?.First());
    }
}