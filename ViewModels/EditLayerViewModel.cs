using System;
using System.Windows.Input;
using Avalonia.Input;
using map_app.Services;
using Mapsui;
using DynamicData;
using Mapsui.Layers;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace map_app.ViewModels
{
    public class EditLayerViewModel : LayerCreationViewModel
    {
        private readonly Map _map;
        private readonly ILayer _toChange;
        private readonly ObservableStack<Action> _undoStack;

        public EditLayerViewModel(Map map, ILayer toChange, ObservableStack<Action> undoStack)
        {
            _map = map;
            _toChange = toChange;
            _undoStack = undoStack;

            Name = toChange.Name;
            Opacity = toChange.Opacity;
            Address = toChange.Attribution.Url;
            
            Cancel = ReactiveCommand.Create<ICloseable>(WindowCloser.Close);
            Confirm = ReactiveCommand.Create<ICloseable>(wnd =>
            {
                if (_toChange.Attribution.Url == Address)
                    EditLayerNameOpacity();
                else
                    CreateNewLayer();
                WindowCloser.Close(wnd);
            },
            this.WhenAnyValue(x => x.Name, x => x.Opacity, x=> x.Address,
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

        private void CreateNewLayer()
        {
            var changed = CreateLayer(Address!, Name!, Opacity);
            var index = _map.Layers.IndexOf(_toChange);
            _map.Layers.Remove(_toChange);
            _map.Layers.Insert(index, changed);
            _undoStack.Push(() =>
            {
                _map.Layers.Remove(changed);
                _map.Layers.Insert(index, _toChange);
                changed.Dispose();
            });
        }

        private void EditLayerNameOpacity()
        {
            var copy = Tuple.Create(_toChange.Name, _toChange.Opacity);
            _toChange.Name = Name!;
            _toChange.Opacity = Opacity;
            _undoStack.Push(() =>
            {
                _toChange.Name = copy.Item1;
                _toChange.Opacity = copy.Item2;
            });
        }
    }
}