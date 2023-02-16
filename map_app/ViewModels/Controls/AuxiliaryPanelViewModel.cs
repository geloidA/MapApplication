using System.ComponentModel.DataAnnotations;
using map_app.Services;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.UI.Avalonia;
using ReactiveUI.Fody.Helpers;

namespace map_app.ViewModels.Controls
{
    public class AuxiliaryPanelViewModel : ViewModelBase
    {
        private RasterizingLayer _currentMapGrid;
        private readonly MapControl _mapControl;
        private double _kilometerInterval;
        private readonly Color LineColor = new Color(150, 150, 150, 150);

        public AuxiliaryPanelViewModel(MapControl mapControl)
        {
            _mapControl = mapControl;
            _mapControl.Navigator!.Navigated += Zoomed;
            KilometerInterval = 1000;
            _currentMapGrid = new RasterizingLayer(new MapGrid(KilometerInterval, LineColor));
        }

        [Reactive]
        [Required(ErrorMessage = "Поле не может быть пустым")]
        [Range(1, 1000, ErrorMessage = "Пожалуйста введите число между 1 и 1000")]
        public double KilometerInterval 
        {
            get => _kilometerInterval;
            set
            {
                _kilometerInterval = value;
                (_currentMapGrid.SourceLayer as MapGrid)?.RerenderLines(_kilometerInterval);
            }
        }

        [Reactive]
        public bool IsGridActivated { get; set; } = false;

        private void ShowGridReference()
        {
            IsGridActivated ^= true;
            if (IsGridActivated)
            {
                _mapControl.Map!.Layers.Add(_currentMapGrid);
                return;
            }
            _mapControl.Map!.Layers.Remove(_currentMapGrid);
        }

        private void Zoomed(object? sender, Mapsui.ChangeType args)
        {
            if (IsGridActivated)
            {
                var kilometerInterval = _mapControl!.Viewport.Resolution / 25;
                (_currentMapGrid.SourceLayer as MapGrid)?.RerenderLines(kilometerInterval);
            }
        }

        private void EnableRuler()
        {
            
        }

        private void ZoomIn() => _mapControl!.Navigator!.ZoomIn(200);

        private void ZoomOut() => _mapControl!.Navigator!.ZoomOut(200);        
    }
}