using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia.Input;
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
        ShowSaveFileDialog = ReactiveCommand.CreateFromTask<ICloseable>(async closeble =>
        {
            var saveLocation = await ShowSaveGraphicsDialog.Handle(Unit.Default);
            if (saveLocation is not null)
            {
                var state = new MapState
                {
                    Name = this.Name,
                    Description = this.Description,
                    Graphics = this._graphics.ToList()
                };
                await MapStateJsonMarshaller.SaveAsync(state, saveLocation);
            }
            WindowCloser.Close(closeble, saveLocation);
        });
        Cancel = ReactiveCommand.Create<ICloseable>(WindowCloser.Close);
    }

    internal Interaction<Unit, string?> ShowSaveGraphicsDialog { get; }

    internal ICommand ShowSaveFileDialog { get; }
    internal ICommand Cancel { get; }
}