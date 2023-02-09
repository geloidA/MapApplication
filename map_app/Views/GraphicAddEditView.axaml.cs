using System.Linq;
using System.Reactive;
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

    private async Task DoShowOpenFileDialogAsync(InteractionContext<Unit, string?> interaction)
    {
        var dialog = new OpenFileDialog();
        var path = await dialog.ShowAsync(this);
        interaction.SetOutput(path?.First());
    }
    
}