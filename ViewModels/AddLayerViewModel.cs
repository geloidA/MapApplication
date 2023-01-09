using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui;

namespace map_app.ViewModels
{
    public class AddLayerViewModel : ViewModelBase
    {
        private readonly Map _map;

        public AddLayerViewModel(Map map)
        {
            _map = map;
        }
    }
}