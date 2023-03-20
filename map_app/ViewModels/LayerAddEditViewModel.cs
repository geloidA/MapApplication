using System;
using System.Linq;
using Mapsui;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Avalonia.Input;
using map_app.Services;
using Mapsui.Layers;
using Mapsui.Tiling.Layers;
using DynamicData;
using map_app.Services.Layers;

namespace map_app.ViewModels
{
    public class LayerAddEditViewModel : ViewModelBase
    {
        private readonly Map _map;
        private readonly ILayer? _toEdit;
        private readonly ObservableStack<Action> _undoStack;
        private readonly IObservable<bool> _canConfirm;

        private LayerAddEditViewModel()
        {
            _canConfirm = this.
                WhenAnyValue(x => x.Name, x=> x.Source,
                    (name, source) => !string.IsNullOrWhiteSpace(name)
                     && (!string.IsNullOrWhiteSpace(source) || _toEdit?.Tag?.ToString() == "Graphic"));
            Cancel = ReactiveCommand.Create<ICloseable>(WindowCloser.Close);
            Confirm = ReactiveCommand.Create<ICloseable>(wnd =>
            {
                if (_toEdit is null)
                    ConfirmAddImpl(wnd);
                else
                    ConfirmEditImpl(wnd);
            }, 
            _canConfirm);
        }

        public LayerAddEditViewModel(Map map, ObservableStack<Action> undoStack) : this()
        {
            _map = map;
            _undoStack = undoStack;
        }

        public LayerAddEditViewModel(Map map, ILayer toEdit, ObservableStack<Action> undoStack) : this(map, undoStack)
        {
            if (toEdit is null) throw new NullReferenceException("Edit layer can't be null");
            _toEdit = toEdit;
            Name = _toEdit.Name;
            Opacity = _toEdit.Opacity;
            Source = _toEdit.Attribution.Url;
        }

        [Reactive]
        public double Opacity { get; set; } = 1;

        [Reactive]
        public string? Name { get; set; }

        [Reactive]
        public string? Source { get; set; }

        public ICommand Confirm { get; }
        public ICommand Cancel { get; }

        private void ConfirmEditImpl(ICloseable wnd)
        {
            if (_toEdit!.Attribution.Url == Source || _toEdit.Tag?.ToString() == "Graphic")
                EditExistedLayer();
            else
                InitializeNewLayer();
            Cancel.Execute(wnd);
        }

        private void InitializeNewLayer()
        {
            var changed = CreateUserLayer(Source!, Name!, Opacity);
            var index = _map.Layers.IndexOf(_toEdit);
            _map.Layers.Remove(_toEdit!);
            _map.Layers.Insert(index, changed);
            _map.Layers.Move(_map.Layers.Count - 1, _map.Layers.FindLayer(nameof(GraphicsLayer)).Single());
            _undoStack.Push(() =>
            {
                _map.Layers.Remove(changed);
                _map.Layers.Insert(index, _toEdit!);
                changed.Dispose();
            });
        }

        private void EditExistedLayer()
        {
            var copy = Tuple.Create(_toEdit!.Name, _toEdit.Opacity);
            _toEdit.Name = Name!;
            _toEdit.Opacity = Opacity;
            _undoStack.Push(() =>
            {
                _toEdit.Name = copy.Item1;
                _toEdit.Opacity = copy.Item2;
            });
        }

        private void ConfirmAddImpl(ICloseable wnd)
        {
            var layer = CreateUserLayer(Source!, Name!, Opacity);
            _map.Layers.Add(layer); // todo: think how get data source
            _undoStack.Push(() => _map.Layers.Remove(_map.Layers.ElementAt(_map.Layers.Count - 1)));
            Cancel.Execute(wnd);            
        }

        private ILayer CreateUserLayer(string address, string name, double opacity)
        {
            return new TileLayer(MyTileSource.Create(address)) { Name = name, Opacity = opacity, Tag = "User" };
        }
    }
}