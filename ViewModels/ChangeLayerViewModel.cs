using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui;
using Mapsui.Layers;

namespace map_app.ViewModels
{
    public class ChangeLayerViewModel : ViewModelBase
    {
        private readonly Map _map;
        private readonly ILayer _toChange;

        public ChangeLayerViewModel(Map map, ILayer toChange)
        {
            _map = map;
            _toChange = toChange;
        }
    }
}