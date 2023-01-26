using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using map_app.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ThemeEditor.Controls.ColorPicker;

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

            var canRemoveGeo = this
                .WhenAnyValue(x => x.SelectedGeoPoint, IsNotNull);

            var canRemoveLinear = this
                .WhenAnyValue(x => x.SelectedLinearPoint, IsNotNull);

            RemoveSelectedGeoPoint = ReactiveCommand.Create(() => GeoPoints.Remove(SelectedGeoPoint!), canRemoveGeo);
            RemoveSelectedLinearPoint = ReactiveCommand.Create(() => LinearPoints.Remove(SelectedLinearPoint!), canRemoveLinear);
        }

        public ObservableCollection<LinearPoint> LinearPoints { get; }

        public ObservableCollection<GeoPoint> GeoPoints { get; }

        [Reactive]
        public LinearPoint? SelectedLinearPoint { get; set; }

        [Reactive]
        public GeoPoint? SelectedGeoPoint { get; set; }

        [Reactive]
        public double Opacity { get; set; }

        public GraphicType GraphicType { get; set; }

        [Reactive]
        public Color? GraphicColor { get; set; }
        

        private void NewGeoPoint() => GeoPoints.Add(new GeoPoint());

        private void NewLinearPoint() => LinearPoints.Add(new LinearPoint());

        public ICommand RemoveSelectedGeoPoint { get; }
        public ICommand RemoveSelectedLinearPoint { get; }

        private static bool IsNotNull(object? obj) => obj != null;
    }
}