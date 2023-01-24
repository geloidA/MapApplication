using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using map_app.Models;

namespace map_app.ViewModels
{
    public class GraphicEditingViewModel : ViewModelBase
    {
        private BaseGraphic _editGraphic;

        public GraphicEditingViewModel(BaseGraphic editGraphic)
        {
            _editGraphic = editGraphic;
        }
    }
}