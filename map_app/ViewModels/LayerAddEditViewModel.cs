using Avalonia.Controls;
using BruTile.Predefined;
using BruTile.Web;
using DynamicData;
using map_app.Services;
using map_app.Services.Layers;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Tiling.Layers;
using MessageBox.Avalonia;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Linq;
using System.Windows.Input;

namespace map_app.ViewModels;

public class LayerAddEditViewModel : ViewModelBase
{
    private readonly Map _map = new();
    private readonly ManagedLayerTag? _layersTag;
    private readonly ILayer? _toEdit;
    private readonly ObservableStack<Action> _undoStack = new();
    private readonly IObservable<bool> _canConfirm;

    private LayerAddEditViewModel()
    {
        _canConfirm = this.
            WhenAnyValue(x => x.Name, x => x.Source,
                (name, source) => !string.IsNullOrWhiteSpace(name) &&
                (!string.IsNullOrWhiteSpace(source) ||
                (!_layersTag?.HaveTileSource ?? false)));
        Cancel = ReactiveCommand.Create<Window>(WindowCloser.Close);
        Confirm = ReactiveCommand.Create<Window>(wnd =>
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
        _layersTag = (_toEdit.Tag as ManagedLayerTag)!;
        Name = _layersTag.Name;
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

    private void ConfirmEditImpl(Window wnd)
    {
        if (_toEdit!.Attribution.Url == Source || !(_toEdit.Tag as ManagedLayerTag)!.HaveTileSource)
            EditExistedLayer();
        else
        {
            if (!Uri.TryCreate(Source, UriKind.Absolute, out _))
            {
                MessageBoxManager.GetMessageBoxStandardWindow(
                    "Некорректные данные",
                    "Неверный адрес")
                .Show();
                return;
            }
            InitializeNewLayer();
        }
        Cancel.Execute(wnd);
    }

    private void InitializeNewLayer()
    {
        var changed = CreateUserLayer(Source!, Name!, Opacity, _toEdit?.Name != "MainTileLayer");
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
        var copy = Tuple.Create(_layersTag!.Name, _toEdit!.Opacity);
        _layersTag.Name = Name;
        _toEdit.Opacity = Opacity;
        _undoStack.Push(() =>
        {
            _layersTag.Name = copy.Item1;
            _toEdit.Opacity = copy.Item2;
        });
    }

    private void ConfirmAddImpl(Window wnd)
    {
        var layer = CreateUserLayer(Source!, Name!, Opacity);
        _map.Layers.Add(layer);
        _undoStack.Push(() => _map.Layers.Remove(_map.Layers.ElementAt(_map.Layers.Count - 1)));
        Cancel.Execute(wnd);
    }

    private static ILayer CreateUserLayer(string address, string name, double opacity, bool canRemove = true)
    {
        var tileSource = new HttpTileSource(new GlobalSphericalMercator(), address) { Attribution = new BruTile.Attribution(url: address) };
        return new TileLayer(tileSource)
        {
            Opacity = opacity,
            Tag = new ManagedLayerTag
            {
                Name = name,
                CanRemove = canRemove,
                HaveTileSource = true
            }
        };
    }
}