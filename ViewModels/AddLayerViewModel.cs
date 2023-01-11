using System;
using System.Linq;
using Mapsui;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Avalonia.Input;
using map_app.Services;

namespace map_app.ViewModels
{
    public class AddLayerViewModel : LayerCreationViewModel
    {
        private readonly Map _map;

        public AddLayerViewModel(Map map, ObservableStack<Action> undoStack)
        {
            _map = map;
            Cancel = ReactiveCommand.Create<ICloseable>(WindowCloser.Close);
            Confirm = ReactiveCommand.Create<ICloseable>(wnd => 
            {
                var layer = CreateLayer(Address!, "User" + Name!, Opacity);
                _map.Layers.Add(layer); // todo: think how get data source
                undoStack.Push(() => _map.Layers.Remove(_map.Layers.ElementAt(_map.Layers.Count - 1)));
                WindowCloser.Close(wnd);
            }, 
            this.WhenAnyValue(
                x => x.Name, x => x.Opacity, x=> x.Address,
                (name, opacity, address) => 
                    !string.IsNullOrEmpty(name) && opacity >= 0.0 && opacity <= 1.0 && !string.IsNullOrEmpty(address)));
        }

        [Reactive]
        public double Opacity { get; set; }

        [Reactive]
        public string? Name { get; set; }

        [Reactive]
        public string? Address { get; set; }

        public ICommand Confirm { get; }
        public ICommand Cancel { get; }
    }
}