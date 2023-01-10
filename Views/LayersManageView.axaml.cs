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
        this.WhenActivated(d => d(ViewModel!.ShowAddDialog.RegisterHandler(DoShowAddDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.ShowChangeDialog.RegisterHandler(DoShowChangeDialogAsync)));
    }

    private async Task DoShowAddDialogAsync(InteractionContext<AddLayerViewModel, LayersManageViewModel> interaction)
    {
        var dialog = new AddLayerView();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<LayersManageViewModel>(this);
        interaction.SetOutput(result);
    }

    private async Task DoShowChangeDialogAsync(InteractionContext<ChangeLayerViewModel, LayersManageViewModel> interaction)
    {
        var dialog = new ChangeLayerView();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<LayersManageViewModel>(this);
        interaction.SetOutput(result);
    }
}