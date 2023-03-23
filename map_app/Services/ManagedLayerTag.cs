using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace map_app.Services;

public class ManagedLayerTag : ReactiveObject
{
    [Reactive]
    public string? Name { get; set; }
    public bool CanRemove { get; set; }
    public bool HaveTileSource { get; set; }
}