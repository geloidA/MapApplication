using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using map_app.ViewModels;
using ReactiveUI;

namespace map_app.Views;

public partial class MapStateSaveView : ReactiveWindow<MapStateSaveViewModel>
{
    public MapStateSaveView()
    {
        InitializeComponent();
        this.WhenActivated(d => d(ViewModel!.ShowSaveGraphicsDialog.RegisterHandler(DoShowSaveGraphicStateDialogAsync)));
    }

    private async Task DoShowSaveGraphicStateDialogAsync(InteractionContext<Unit, string?> interaction)
    {
        var dialog = new SaveFileDialog();
        dialog.DefaultExtension = "json";
        var result = await dialog.ShowAsync(this);
        interaction.SetOutput(result);
    }

}