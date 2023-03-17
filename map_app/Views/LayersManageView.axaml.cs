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
        this.WhenActivated(d => d(ViewModel!.ShowAddEditDialog.RegisterHandler(x => this.ShowDialogAsync(x, new LayerAddEditView()))));
    }
}