using map_app.Services;
using Mapsui.Layers;
using Mapsui.Tiling.Layers;

namespace map_app.ViewModels
{
    public class LayerCreationViewModel : ViewModelBase
    {
        protected ILayer CreateUserLayer(string address, string name, double opacity)
        {
            return new TileLayer(DictKnownTileSources.Create(address)) { Name = name, Opacity = opacity, Tag="User" };
        }
    }
}