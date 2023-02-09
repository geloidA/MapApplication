using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
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
        var dialog = new LayerAddEditView();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<LayersManageViewModel>(this);
        interaction.SetOutput(result);
    }
}