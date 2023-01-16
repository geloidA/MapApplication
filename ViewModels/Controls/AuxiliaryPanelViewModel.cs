using map_app.Services;
using map_app.ViewModels;
using Mapsui.UI.Avalonia;

namespace map_app.ViewModels.Controls
{
    public class AuxiliaryPanelViewModel : ViewModelBase
    {

        private bool _gridIsActive = true;
        private readonly MapControl _mapControl;

        public AuxiliaryPanelViewModel(MapControl mapControl)
        {
            _mapControl = mapControl;
        }

        private void ShowGridReference()
        {
            if (_gridIsActive)
            {
                _mapControl.Map!.Layers.Add(GridReference.Grid);
                _gridIsActive = false;
                return;
            }

            _mapControl.Map!.Layers.Remove(GridReference.Grid);
            _gridIsActive = true;
        }

        private void EnableRuler()
        {
            
        }

        private void ZoomIn() => _mapControl!.Navigator!.ZoomIn(200);

        private void ZoomOut() => _mapControl!.Navigator!.ZoomOut(200);        
    }
}