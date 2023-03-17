using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;

namespace map_app.Views;

public static class WindowExtentions
{
    public static async Task ShowDialogAsync<TInput, TOutput>(this Window owner, InteractionContext<TInput, TOutput> interaction, Window dialog) 
        where TInput : ReactiveObject
    {
        dialog.DataContext = interaction.Input;
        interaction.SetOutput(await dialog.ShowDialog<TOutput>(owner));
    }
}