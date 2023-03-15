using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using map_app.ViewModels;
using ReactiveUI;

namespace map_app.Views;

public partial class LayersManageView : ReactiveWindow<LayersManageViewModel>
{
    public LayersManageView()
    {
        InitializeComponent();
        this.WhenActivated(d => d(ViewModel!.ShowAddEditDialog.RegisterHandler(DoShowAddDialogAsync)));
    }

    private async Task DoShowAddDialogAsync(InteractionContext<LayerAddEditViewModel, LayersManageViewModel> interaction)
    {
        var dialog = new LayerAddEditView { DataContext = interaction.Input };
        interaction.SetOutput(await dialog.ShowDialog<LayersManageViewModel>(this));
    }
}