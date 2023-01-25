using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using map_app.Models;
using ReactiveUI.Fody.Helpers;

namespace map_app.ViewModels
{
    public class GraphicEditingViewModel : ViewModelBase
    {
        private BaseGraphic _editGraphic;

        public GraphicEditingViewModel(BaseGraphic editGraphic)
        {
            _editGraphic = editGraphic;
            LinearPoints = new ObservableCollection<LinearPoint>(_editGraphic.LinearPoints);
            GeoPoints = new ObservableCollection<GeoPoint>(_editGraphic.GeoPoints);
            Opacity = _editGraphic.Opacity;
            GraphicType = _editGraphic.Type;
            GraphicColor = new Color(
                a: (byte)_editGraphic.Color!.A,
                r: (byte)_editGraphic.Color!.R,
                g: (byte)_editGraphic.Color!.G,
                b: (byte)_editGraphic.Color!.B
            );
        }

        public ObservableCollection<LinearPoint> LinearPoints { get; }

        public ObservableCollection<GeoPoint> GeoPoints { get; }

        [Reactive]
        public double Opacity { get; set; }

        public GraphicType GraphicType { get; set; }

        [Reactive]
        public Color? GraphicColor { get; set; }
    }
}