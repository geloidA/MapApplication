using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using map_app.Models;
using map_app.Services;
using map_app.Services.IO;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace map_app.ViewModels;

public class MapStateSaveViewModel : ViewModelBase
{
    [Reactive]
    public string? Name { get; set; }

    [Reactive]
    public string? Description { get; set; }

    private IEnumerable<BaseGraphic> _graphics;

    public MapStateSaveViewModel(IEnumerable<BaseGraphic> graphics)
    {
        _graphics = graphics;
        ShowSaveGraphicsDialog = new Interaction<Unit, string?>();
        ShowSaveFileDialog = ReactiveCommand.CreateFromTask<Window>(async closeble =>
        {
            MapState? state = null;
            var saveLocation = await ShowSaveGraphicsDialog.Handle(Unit.Default);
            if (saveLocation is not null)
            {
                state = new MapState
                {
                    Name = this.Name,
                    Description = this.Description,
                    Graphics = this._graphics.ToList(),
                    FileLocation = saveLocation
                };
                await MapStateJsonMarshaller.SaveAsync(state, saveLocation);
            }
            WindowCloser.Close(closeble, state);
        });
        Cancel = ReactiveCommand.Create<Window>(WindowCloser.Close);
    }

    internal Interaction<Unit, string?> ShowSaveGraphicsDialog { get; }

    internal ICommand ShowSaveFileDialog { get; }
    internal ICommand Cancel { get; }
}