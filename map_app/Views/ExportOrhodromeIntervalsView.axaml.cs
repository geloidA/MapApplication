using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using map_app.ViewModels;
using ReactiveUI;

namespace map_app.Views;

public partial class ExportOrhodromeIntervalsView : ReactiveWindow<ExportOrhodromeIntervalsViewModel>
{
    public ExportOrhodromeIntervalsView()
    {
        InitializeComponent();
        this.WhenActivated(d => d(ViewModel!.ShowSaveFileDialog.RegisterHandler(DoShowSaveDialogAsync)));
    }

    private async Task DoShowSaveDialogAsync(InteractionContext<Unit, string?> interaction)
    {
        var dialog = new SaveFileDialog();
        dialog.DefaultExtension = "csv";
        var result = await dialog.ShowAsync(this);
        interaction.SetOutput(result);
    }
}