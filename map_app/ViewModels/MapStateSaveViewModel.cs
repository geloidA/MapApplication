using Avalonia.Controls;
using map_app.Models;
using map_app.Services;
using map_app.Services.IO;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace map_app.ViewModels;

public class MapStateSaveViewModel : ViewModelBase
{
    [Reactive]
    public string? Name { get; set; }

    [Reactive]
    public string? Description { get; set; }

    private readonly IEnumerable<BaseGraphic> _graphics;

    public MapStateSaveViewModel(IEnumerable<BaseGraphic> graphics)
    {
        _graphics = graphics;
        ShowSaveFileDialog = ReactiveCommand.CreateFromTask<Window>(async wnd =>
        {
            MapState? state = null;
            var saveLocation = await ShowSaveGraphicsDialog.Handle(Unit.Default);
            if (saveLocation is not null)
            {
                state = new MapState
                {
                    Name = Name,
                    Description = Description,
                    Graphics = _graphics.ToList(),
                    FileLocation = saveLocation
                };
                if (await TrySaveState(state, saveLocation) == false)
                {
                    await MessageBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandardWindow("Ошибка", "Не удалось сохранить состояние")
                        .ShowDialog(wnd);
                    return;
                }
            }
            WindowCloser.Close(wnd, state);
        });
        Cancel = ReactiveCommand.Create<Window>(WindowCloser.Close);
    }

    private async Task<bool> TrySaveState(MapState state, string saveLocation)
    {
        try
        {
            await MapStateJsonMarshaller.SaveAsync(state, saveLocation);
            return true;
        }
        catch
        {
            return false;
        }
    }

    internal Interaction<Unit, string?> ShowSaveGraphicsDialog { get; } = new();

    internal ICommand ShowSaveFileDialog { get; }
    internal ICommand Cancel { get; }
}