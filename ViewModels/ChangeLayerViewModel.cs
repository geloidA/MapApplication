using System;
using System.Windows.Input;
using Avalonia.Input;
using BruTile.Web;
using map_app.Services;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Tiling.Layers;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace map_app.ViewModels
{
    public class ChangeLayerViewModel : ViewModelBase
    {
        private readonly Map _map;
        private readonly ILayer _toChange;

        public ChangeLayerViewModel(Map map, ILayer toChange, ObservableStack<Action> undoStack)
        {
            _map = map;
            _toChange = toChange;
            Name = toChange.Name;
            Opacity = toChange.Opacity;
            
            Cancel = ReactiveCommand.Create<ICloseable>(CommonFunctionality.CloseView);
            Confirm = ReactiveCommand.Create<ICloseable>(wnd =>
            {
                var copy = Tuple.Create(_toChange.Name, _toChange.Opacity);
                _toChange.Name = Name;
                _toChange.Opacity = Opacity;
                undoStack.Push(() =>
                {
                    _toChange.Name = copy.Item1;
                    _toChange.Opacity = copy.Item2;
                });
                CommonFunctionality.CloseView(wnd);
            },
            this.WhenAnyValue(x => x.Name, n => !string.IsNullOrEmpty(n)));
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